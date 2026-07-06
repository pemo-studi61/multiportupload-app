// file: src/MultiPortUpload.Infrastructure/Options/BenchmarkStorageOptions.cs

namespace MultiPortUpload.Infrastructure.Options;

/// <summary>
/// Konfiguration für die PostgreSQL-gestützte Persistenz der Benchmark-Daten.
/// Wird aus dem Abschnitt "BenchmarkStorage" der Konfiguration gebunden.
/// </summary>
public sealed class BenchmarkStorageOptions
{
    public const string SectionName = "BenchmarkStorage";

    /// <summary>
    /// Aktiviert die Persistenz. Ist sie deaktiviert, wird ein No-Op-Store
    /// registriert, sodass die Anwendung auch ohne erreichbare Datenbank läuft.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Npgsql-Verbindungszeichenfolge zur PostgreSQL-Datenbank.</summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// Wenn true, werden ausstehende EF-Core-Migrationen beim Start angewendet.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; } = true;
}
