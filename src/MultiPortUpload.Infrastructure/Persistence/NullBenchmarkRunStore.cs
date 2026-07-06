// file: src/MultiPortUpload.Infrastructure/Persistence/NullBenchmarkRunStore.cs

using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Infrastructure.Persistence;

/// <summary>
/// No-Op-Implementierung des <see cref="IBenchmarkRunStore"/>. Wird registriert,
/// wenn die Benchmark-Persistenz deaktiviert ist (BenchmarkStorage:Enabled = false),
/// damit die Anwendung auch ohne erreichbare PostgreSQL-Datenbank lauffähig bleibt.
/// </summary>
public sealed class NullBenchmarkRunStore : IBenchmarkRunStore
{
    public Task<BenchmarkRun> CreateAsync(BenchmarkRun run, CancellationToken cancellationToken = default)
        => Task.FromResult(run);

    public Task<IReadOnlyList<BenchmarkRun>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BenchmarkRun>>(Array.Empty<BenchmarkRun>());

    public Task<BenchmarkRun?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult<BenchmarkRun?>(null);

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(false);
}
