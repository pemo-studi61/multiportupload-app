// file: src/MultiPortUpload.Infrastructure/Persistence/EfStoredArtifactStore.cs

using Microsoft.EntityFrameworkCore;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL-Implementierung des <see cref="IStoredArtifactStore"/> auf Basis von
/// EF Core.
/// </summary>
public sealed class EfStoredArtifactStore : IStoredArtifactStore
{
    private readonly BenchmarkDbContext _dbContext;

    public EfStoredArtifactStore(BenchmarkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StoredArtifact> CreateAsync(StoredArtifact artifact, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        await _dbContext.StoredArtifacts.AddAsync(artifact, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Nach SaveChanges enthält die Instanz die von der DB generierten Werte
        // (CreatedAtUtc).
        return artifact;
    }

    public async Task<IReadOnlyList<StoredArtifact>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            limit = 100;
        }

        return await _dbContext.StoredArtifacts
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<StoredArtifact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StoredArtifacts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
