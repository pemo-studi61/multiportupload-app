// file: src/MultiPortUpload.Infrastructure/Persistence/EfBenchmarkRecordStore.cs

using Microsoft.EntityFrameworkCore;
using MultiPortUpload.Application.Models;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL-Implementierung des <see cref="IBenchmarkRecordStore"/> auf Basis
/// von EF Core.
/// </summary>
public sealed class EfBenchmarkRecordStore : IBenchmarkRecordStore
{
    private readonly BenchmarkDbContext _dbContext;

    public EfBenchmarkRecordStore(BenchmarkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BenchmarkRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await _dbContext.BenchmarkRecords.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BenchmarkRecord>> GetRecentAsync(
        int limit = 100,
        string? variant = null,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            limit = 100;
        }

        var query = _dbContext.BenchmarkRecords
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(variant))
        {
            query = query.Where(r => r.UploadVariant == variant);
        }

        return await query
            .OrderByDescending(r => r.StartedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BenchmarkRecord>> GetByRunUuidAsync(
        Guid runUuid,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BenchmarkRecords
            .AsNoTracking()
            .Where(r => r.BenchmarkRunId == runUuid)
            .OrderBy(r => r.StartedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BenchmarkVariantSummary>> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        // p95 wird über PostgreSQLs percentile_cont berechnet, das sich nicht über
        // LINQ ausdrücken lässt – daher eine rohe SQL-Abfrage. Die Spalten-Aliase
        // entsprechen exakt den Property-Namen von BenchmarkVariantSummary, damit
        // EF Core das Ergebnis materialisieren kann.
        const string sql = """
            SELECT
                upload_variant AS "UploadVariant",
                COUNT(*) AS "UploadCount",
                AVG(duration_ms)::double precision AS "AverageDurationMs",
                MIN(duration_ms) AS "MinDurationMs",
                MAX(duration_ms) AS "MaxDurationMs",
                percentile_cont(0.95) WITHIN GROUP (ORDER BY duration_ms) AS "P95DurationMs",
                SUM(size_in_bytes)::bigint AS "TotalBytes"
            FROM benchmark_records
            GROUP BY upload_variant
            ORDER BY upload_variant;
            """;

        return await _dbContext.Database
            .SqlQueryRaw<BenchmarkVariantSummary>(sql)
            .ToListAsync(cancellationToken);
    }
}
