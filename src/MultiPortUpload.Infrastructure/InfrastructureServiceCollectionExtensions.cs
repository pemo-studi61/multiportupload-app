// file: /src/MultiPortUpload.Infrastructure/InfrastructureServiceCollectionExtensions.cs

namespace MultiPortUpload.Infrastructure;

using Amazon;
using Amazon.S3;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Infrastructure.Options;
using MultiPortUpload.Infrastructure.Adapters.Storage;
using MultiPortUpload.Infrastructure.Persistence;
using MultiPortUpload.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var s3Section = configuration.GetSection("S3Storage");

        Console.WriteLine($"[DEBUG] S3 Endpoint = {s3Section["Endpoint"]}");
        Console.WriteLine($"[DEBUG] S3 BucketName = {s3Section["BucketName"]}");

        services.Configure<S3StorageOptions>(s3Section);

        AddBenchmarkPersistence(services, configuration);

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<S3StorageOptions>>().Value;

            // Zu Testzwecken nur ein Http-Endpunkt, damit es mit MinIO lokal funktioniert
            var config = new AmazonS3Config
            {
                ServiceURL = options.Endpoint,
                ForcePathStyle = options.UsePathStyle,
                UseHttp = true,
                AuthenticationRegion = "us-east-1",
                RegionEndpoint = RegionEndpoint.USEast1
            };

            return new AmazonS3Client(
                options.AccessKey,
                options.SecretKey,
                config);
        });

        // Zentrale Registrierung aller Upload-Adapter – die EINZIGE Stelle, an der
        // Adapter verdrahtet werden. Pro Adapter wird jedes Interface registriert,
        // das er implementiert:
        //   - IUploadAdapter  : speist die UploadAdapterRegistry und damit die
        //                       Liste unter GET /api/upload-adapters.
        //   - IUploadPort     : wird von der UploadPortFactory anhand der Variant
        //                       aufgelöst (klassischer Single-Shot-Upload).
        //   - IChunkedUploadPort / IPresignedUploadPort : Spezial-Ports für die
        //                       jeweiligen Endpunkte.
        // Wer einen neuen Adapter ergänzt, fügt hier genau eine Gruppe hinzu.
        services.AddScoped<IUploadAdapterRegistry, UploadAdapterRegistry>();

        AddUploadAdapter<LocalFileUploadAdapter>(services, asUploadPort: true);
        AddUploadAdapter<StreamingUploadAdapter>(services, asUploadPort: true);
        AddUploadAdapter<MemoryUploadAdapter>(services, asUploadPort: true);
        AddUploadAdapter<HashingUploadAdapter>(services, asUploadPort: true);
        AddUploadAdapter<VirusScanMockUploadAdapter>(services, asUploadPort: true);
        AddUploadAdapter<S3UploadAdapter>(services, asUploadPort: true);
        AddUploadAdapter<QueueBasedUploadAdapter>(services, asUploadPort: true);
        AddUploadAdapter<ResumableUploadAdapter>(services, asUploadPort: true);

        // ChunkedUploadAdapter und S3PresignedUploadAdapter implementieren KEIN
        // IUploadPort, sondern eigene Spezial-Ports. Sie werden nur als
        // IUploadAdapter (für die Liste) plus ihrem jeweiligen Port registriert.
        AddUploadAdapter<ChunkedUploadAdapter>(services, asUploadPort: false);
        services.AddScoped<IChunkedUploadPort, ChunkedUploadAdapter>();

        AddUploadAdapter<S3PresignedUploadAdapter>(services, asUploadPort: false);
        services.AddScoped<IPresignedUploadPort, S3PresignedUploadAdapter>();

        return services;
    }

    // Registriert einen Upload-Adapter für alle Standard-Interfaces: immer als
    // IUploadAdapter (für die Registry/Liste) und – sofern asUploadPort gesetzt –
    // zusätzlich als IUploadPort (für die UploadPortFactory). Alle Interface-
    // Auflösungen teilen sich innerhalb eines Scopes dieselbe Instanz, da sie über
    // den konkreten Typ aufgelöst werden.
    private static void AddUploadAdapter<T>(IServiceCollection services, bool asUploadPort)
        where T : class, IUploadAdapter
    {
        services.AddScoped<T>();
        services.AddScoped<IUploadAdapter>(sp => sp.GetRequiredService<T>());

        if (asUploadPort)
        {
            services.AddScoped<IUploadPort>(sp => (IUploadPort)sp.GetRequiredService<T>());
        }
    }

    // Registriert die PostgreSQL-gestützte Benchmark-Persistenz. Ist sie in der
    // Konfiguration deaktiviert (oder fehlt eine Verbindungszeichenfolge), wird
    // stattdessen ein No-Op-Store registriert, damit die API auch ohne erreichbare
    // Datenbank startet.
    private static void AddBenchmarkPersistence(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(BenchmarkStorageOptions.SectionName);
        services.Configure<BenchmarkStorageOptions>(section);

        var options = section.Get<BenchmarkStorageOptions>() ?? new BenchmarkStorageOptions();

        if (!options.Enabled || string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            Console.WriteLine(
                "[DEBUG] Benchmark-Persistenz deaktiviert oder ohne ConnectionString – verwende NullBenchmarkRecordStore.");
            services.AddSingleton<IBenchmarkRecordStore, NullBenchmarkRecordStore>();
            services.AddSingleton<IBenchmarkRunStore, NullBenchmarkRunStore>();
            services.AddSingleton<IStoredArtifactStore, NullStoredArtifactStore>();
            return;
        }

        Console.WriteLine("[DEBUG] Benchmark-Persistenz aktiviert (PostgreSQL).");

        services.AddDbContext<BenchmarkDbContext>(dbOptions =>
            dbOptions.UseNpgsql(options.ConnectionString, npgsql =>
                // Resilienz für gehostete Datenbanken (z. B. Supabase): transiente
                // Verbindungsfehler werden automatisch erneut versucht.
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null)));

        services.AddScoped<IBenchmarkRecordStore, EfBenchmarkRecordStore>();
        services.AddScoped<IBenchmarkRunStore, EfBenchmarkRunStore>();
        services.AddScoped<IStoredArtifactStore, EfStoredArtifactStore>();
    }
}