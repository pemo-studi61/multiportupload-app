// file: /src/MultiPortUpload.Api/Program.cs
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Exceptions;
using MultiPortUpload.Application.Services;
using MultiPortUpload.Api.Endpoints;
using MultiPortUpload.Api.Middleware;
using MultiPortUpload.Infrastructure;
using MultiPortUpload.Infrastructure.Adapters.Storage;
using MultiPortUpload.Infrastructure.BackgroundServices;
using MultiPortUpload.Infrastructure.Options;
using MultiPortUpload.Infrastructure.Persistence;
using MultiPortUpload.Infrastructure.Queue;
using MultiPortUpload.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// PM: 02/06/2026 - Logging auf NLog umgestellt. NLog ist als alleiniger Logging-
// Provider verdrahtet (UseNLog) und lädt NLog.config aus dem Anwendungsverzeichnis.
// Die Dateiziele verwenden relative Pfade ("logs/..."), sodass die Logs sowohl
// lokal (dotnet run) als auch im Container (WORKDIR /app -> /app/logs) geschrieben
// werden.
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers();

// PM: 09/06/2026 - OpenTelemetry-Metriken (Minimal-Setup). Ziel ist ein Sanity-Check
// des Speicherverbrauchs während großer Benchmark-Läufe: Wie viel verwalteten Speicher
// hält die CLR (GC-Heap je Generation), wie hoch ist der Working-Set des Prozesses und
// wie oft/teuer läuft die Garbage Collection. Die Werte werden im Prometheus-Format
// unter /metrics bereitgestellt (siehe MapPrometheusScrapingEndpoint weiter unten) und
// können per `curl http://localhost:8080/metrics` oder einem Prometheus-Scrape gelesen
// werden. Es werden bewusst KEINE OTLP-Collector-/Grafana-Container ergänzt.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "MultiPortUpload.Api",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"))
    .WithMetrics(metrics => metrics
        // CLR/GC: gc.heap.size je Generation, Allokationsrate, GC-Pausen, Heap-Fragmentierung.
        .AddRuntimeInstrumentation()
        // Prozessebene: process.memory.usage (Working Set) und process.memory.virtual.
        .AddProcessInstrumentation()
        // HTTP-Last, um Speicher mit dem aktuellen Anfrageaufkommen korrelieren zu können.
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

builder.Services.Configure<LocalStorageOptions>(
    builder.Configuration.GetSection(LocalStorageOptions.SectionName));

// PM: 06/05/2026 - Herausgenommen, da es jetzt builder.Services.AddInfrastructure(builder.Configuration); gibt
// builder.Services.Configure<S3StorageOptions>(builder.Configuration.GetSection("S3"));

builder.Services.AddScoped<IUploadPortFactory, UploadPortFactory>();
builder.Services.AddScoped<BenchmarkRecordingService>();
builder.Services.AddScoped<StoredArtifactRecordingService>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<ResumableUploadSessionService>();

// PM: 12/06/2026 - Sämtliche Upload-Adapter (inkl. ihrer IUploadAdapter-/IUploadPort-
// Registrierungen und der UploadAdapterRegistry) werden jetzt zentral in
// AddInfrastructure verdrahtet. Dadurch gibt es nur noch EINE Stelle, an der ein
// neuer Adapter ergänzt werden muss.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<UploadQueueWorker>();

// 6.5.2026 - eingefügt, damit der S3-Client mit den konfigurierten Optionen erstellt und als Singleton bereitgestellt wird
// noch nicht getestet, ob das so funktioniert, da die S3-Optionen erst später in der Entwicklung hinzugefügt wurden
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var options = sp.GetRequiredService<IOptions<S3StorageOptions>>().Value;

    var config = new AmazonS3Config
    {
        ServiceURL = options.Endpoint,
        ForcePathStyle = options.UsePathStyle,
        UseHttp = options.Endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
    };

    return new AmazonS3Client(
        options.AccessKey,
        options.SecretKey,
        config);
});

builder.Services.AddSingleton<IUploadQueue, InMemoryUploadQueue>();

// Einfügt für Swagger/OpenAPI-Dokumentation (optional, aber hilfreich für Tests und Entwicklung)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PM: 04/06/2026 - CORS-Policy hinzugefügt, damit die Admin-Frontend-Anwendung, die auf http://localhost:5173 läuft, Anfragen an die API senden kann. Die Policy erlaubt alle Header und Methoden von diesem Ursprung. In einer Produktionsumgebung sollte die CORS-Policy restriktiver sein und nur die tatsächlich benötigten Ursprünge, Header und Methoden erlauben.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AdminFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://209.38.245.50",
                "http://209.38.245.50:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("AdminFrontend");

var logger = app.Services
       .GetRequiredService<ILogger<Program>>();

logger.LogInformation("*** Starting MultiPortUpload API...");

// PM: 02/06/2026 - Benchmark-Persistenz (PostgreSQL): ausstehende EF-Core-Migrationen
// beim Start anwenden, sofern aktiviert. Fehlt die Datenbank, wird nur gewarnt, damit
// die API trotzdem startet (Uploads funktionieren auch ohne Persistenz).
using (var scope = app.Services.CreateScope())
{
    var benchmarkOptions = scope.ServiceProvider
        .GetRequiredService<IOptions<BenchmarkStorageOptions>>().Value;

    if (benchmarkOptions is { Enabled: true, ApplyMigrationsOnStartup: true }
        && !string.IsNullOrWhiteSpace(benchmarkOptions.ConnectionString))
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BenchmarkDbContext>();
            dbContext.Database.Migrate();
            logger.LogInformation("*** Benchmark-Datenbankschema ist aktuell (Migrationen angewendet).");
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "*** Benchmark-Datenbankmigration fehlgeschlagen. Die API startet ohne Persistenz.");
        }
    }
}

// PM: 12/05/26 - eingefügt für zentrales Logging (eventuell etwas spät)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

// PM: 04/06/2026 - Eingefügt, damit die API auch statische Dateien aus dem wwwroot-Verzeichnis bereitstellen kann. Das ist nützlich, um z.B. eine einfache Startseite oder Testdateien bereitzustellen. In diesem Fall dient es dazu, die index.html als Startseite der API
app.UseDefaultFiles();
app.UseStaticFiles();

// PM: 09/06/2026 - Prometheus-Scrape-Endpunkt der OpenTelemetry-Metriken. Stellt die
// gesammelten CLR-/GC- und Prozess-Speicherkennzahlen unter GET /metrics im
// Prometheus-Textformat bereit.
app.MapPrometheusScrapingEndpoint();

app.MapControllers();
app.MapHealthEndpoints();
app.MapUploadEndpoints();
app.MapBenchmarkEndpoints();
app.MapStoredArtifactEndpoints();
app.MapSystemEndpoints();
app.MapUploadAdapterEndpoints();

// Für den Aufruf des Adminpanels wird die index.html im wwwroot-Verzeichnis bereitgestellt, damit die Admin-Frontend-Anwendung, die mit React erstellt wurde, als statische Datei von der API bereitgestellt und unter http://localhost:8080/ (oder der entsprechenden URL) erreichbar ist. Das ermöglicht es, die Admin-Oberfläche direkt über die API zu hosten, ohne einen separaten Webserver oder eine separate Hosting-Lösung für das Frontend zu benötigen.
app.MapFallbackToFile("/admin/{*path:nonfile}", "admin/index.html");

// Swagger-Middleware hinzufügen, damit die API-Dokumentation unter /swagger verfügbar ist. Dies ist besonders nützlich für Tests und Entwicklung, um die verfügbaren Endpunkte und deren Spezifikationen zu sehen und direkt aus der Dokumentation heraus zu testen.
app.UseSwagger();
app.UseSwaggerUI();

// Nur für Loggingtests
app.MapGet("/debug/error", () =>
{
     throw new UploadFailedException(
        "Dies ist ein Testfehler der ExceptionHandlingMiddleware."
     );
});

try
{
    app.Run();
}
finally
{
    // Gepufferte Log-Einträge beim Beenden sicher schreiben.
    LogManager.Shutdown();
}