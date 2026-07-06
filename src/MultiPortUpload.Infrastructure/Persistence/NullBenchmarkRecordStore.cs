// file: src/MultiPortUpload.Infrastructure/Persistence/NullBenchmarkRecordStore.cs

using MultiPortUpload.Application.Models;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Infrastructure.Persistence;

/// <summary>
/// No-Op-Implementierung des <see cref="IBenchmarkRecordStore"/>. Wird registriert,
/// wenn die Benchmark-Persistenz deaktiviert ist (BenchmarkStorage:Enabled = false),
/// damit die Anwendung auch ohne erreichbare PostgreSQL-Datenbank lauffähig bleibt.
/// </summary>
public sealed class NullBenchmarkRecordStore : IBenchmarkRecordStore
{
    public Task AddAsync(BenchmarkRecord record, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<BenchmarkRecord>> GetRecentAsync(
        int limit = 100,
        string? variant = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BenchmarkRecord>>(Array.Empty<BenchmarkRecord>());

    public Task<IReadOnlyList<BenchmarkRecord>> GetByRunUuidAsync(
        Guid runUuid,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BenchmarkRecord>>(Array.Empty<BenchmarkRecord>());

    public Task<IReadOnlyList<BenchmarkVariantSummary>> GetSummaryAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BenchmarkVariantSummary>>(Array.Empty<BenchmarkVariantSummary>());
}
