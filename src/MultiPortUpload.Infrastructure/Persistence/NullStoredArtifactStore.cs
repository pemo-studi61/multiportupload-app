// file: src/MultiPortUpload.Infrastructure/Persistence/NullStoredArtifactStore.cs

using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Infrastructure.Persistence;

/// <summary>
/// No-Op-Implementierung des <see cref="IStoredArtifactStore"/>. Wird registriert,
/// wenn die Persistenz deaktiviert ist (BenchmarkStorage:Enabled = false), damit die
/// Anwendung auch ohne erreichbare PostgreSQL-Datenbank lauffähig bleibt.
/// </summary>
public sealed class NullStoredArtifactStore : IStoredArtifactStore
{
    public Task<StoredArtifact> CreateAsync(StoredArtifact artifact, CancellationToken cancellationToken = default)
        => Task.FromResult(artifact);

    public Task<IReadOnlyList<StoredArtifact>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StoredArtifact>>(Array.Empty<StoredArtifact>());

    public Task<StoredArtifact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<StoredArtifact?>(null);
}
