// file: src/MultiPortUpload.Infrastructure/Persistence/BenchmarkDbContextFactory.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MultiPortUpload.Infrastructure.Persistence;

/// <summary>
/// Design-Time-Factory, damit "dotnet ef migrations" einen DbContext erzeugen
/// kann, ohne die Anwendung zu starten. Die Verbindungszeichenfolge kann über die
/// Umgebungsvariable BENCHMARK_DB_CONNECTION überschrieben werden; andernfalls wird
/// ein lokaler Standardwert verwendet (nur für Migrations-/Design-Zwecke).
/// </summary>
public sealed class BenchmarkDbContextFactory : IDesignTimeDbContextFactory<BenchmarkDbContext>
{
    public BenchmarkDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("BENCHMARK_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=multiportupload;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<BenchmarkDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new BenchmarkDbContext(optionsBuilder.Options);
    }
}
