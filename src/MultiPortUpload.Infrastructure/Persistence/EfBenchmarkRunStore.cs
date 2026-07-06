// file: src/MultiPortUpload.Infrastructure/Persistence/EfBenchmarkRunStore.cs

using Microsoft.EntityFrameworkCore;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL-Implementierung des <see cref="IBenchmarkRunStore"/> auf Basis von
/// EF Core.
/// </summary>
public sealed class EfBenchmarkRunStore : IBenchmarkRunStore
{
    private readonly BenchmarkDbContext _dbContext;

    public EfBenchmarkRunStore(BenchmarkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BenchmarkRun> CreateAsync(BenchmarkRun run, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);

        await _dbContext.BenchmarkRuns.AddAsync(run, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Nach SaveChanges enthält die Instanz die von der DB generierten Werte
        // (Id, CreatedAt, RunUuid).
        return run;
    }

    public async Task<IReadOnlyList<BenchmarkRun>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            limit = 100;
        }

        return await _dbContext.BenchmarkRuns
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<BenchmarkRun?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.BenchmarkRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var affected = await _dbContext.BenchmarkRuns
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return affected > 0;
    }
}
