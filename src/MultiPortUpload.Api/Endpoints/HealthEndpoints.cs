// file: src/MultiPortUpload.Api/Endpoints/HealthEndpoints.cs

using MultiPortUpload.Infrastructure.Persistence;

namespace MultiPortUpload.Api.Endpoints;

public static class HealthEndpoints
{
    private const int DatabaseCheckAttempts = 3;
    private const int DatabaseCheckDelayMilliseconds = 250;

    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet(
        "/health",
        // PM: 09/06/2026 - BenchmarkDbContext optional über den ServiceProvider auflösen
        // statt direkt als Parameter zu injizieren. Bei deaktivierter Benchmark-Persistenz
        // (BenchmarkStorage:Enabled = false) ist der DbContext nicht registriert; eine
        // direkte Injektion ließ die Minimal-API den Parameter als Request-Body deuten und
        // brach den App-Start ab ("Body was inferred ..."). Die API läuft laut Design auch
        // ohne Persistenz, daher wird der DB-Check in diesem Fall als "Disabled" gemeldet.
        async (IServiceProvider services) =>
        {
            var dbContext = services.GetService<BenchmarkDbContext>();

            var databaseEnabled = dbContext is not null;
            var databaseOk = databaseEnabled && await CanConnectWithRetryAsync(dbContext!);

            var logsOk = Directory.Exists("logs");
            var uploadsOk = Directory.Exists("uploads");

            // Deaktivierte Persistenz verschlechtert den Gesamtstatus nicht.
            // Ein aktiver, aber momentan nicht erreichbarer DB-Check wird als Warning
            // gemeldet, damit kurzfristige Pooler-/Auth-Probleme nicht direkt als Error
            // erscheinen.
            var databaseHealthy = !databaseEnabled || databaseOk;

            return Results.Ok(new
            {
                status = databaseHealthy && logsOk && uploadsOk
                ? "OK"
                : "Warning",
                service = "MultiPortUpload",
                version = "0.3",
                timestamp = DateTime.UtcNow,

                checks = new
                {
                    postgreSql = new
                    {
                        status = !databaseEnabled ? "Disabled" : databaseOk ? "OK" : "Warning"
                    },

                    logs = new
                    {
                        status = logsOk ? "OK" : "Error",
                        fileCount = logsOk  ? Directory.GetFiles("logs", "*.log").Length : 0
                    },
                    uploads = new
                    {
                        status = uploadsOk ? "OK" : "Error",
                        fileCount = uploadsOk
                            ? Directory.GetFiles("uploads").Length
                            : 0
                    },
                }
            });
        })
        .WithName("Health")
        .WithTags("Health");
    }

    private static async Task<bool> CanConnectWithRetryAsync(BenchmarkDbContext dbContext)
    {
        for (var attempt = 1; attempt <= DatabaseCheckAttempts; attempt++)
        {
            try
            {
                if (await dbContext.Database.CanConnectAsync())
                {
                    return true;
                }
            }
            catch
            {
                // Beim nächsten Versuch erneut pruefen.
            }

            if (attempt < DatabaseCheckAttempts)
            {
                await Task.Delay(DatabaseCheckDelayMilliseconds);
            }
        }

        return false;
    }
}