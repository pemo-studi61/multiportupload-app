// file: src/MultiPortUpload.Application/Ports/Outbound/IStoredArtifactStore.cs

using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Application.Ports.Outbound;

/// <summary>
/// Outbound-Port zum Anlegen und Abfragen von Datei-Metadaten
/// (<see cref="StoredArtifact"/>). Die konkrete Implementierung (PostgreSQL via
/// EF Core) liegt in der Infrastructure-Schicht.
/// </summary>
public interface IStoredArtifactStore
{
    /// <summary>
    /// Legt einen neuen Artefakt-Datensatz an und liefert ihn inklusive der von der
    /// Datenbank generierten Werte (CreatedAtUtc) zurück.
    /// </summary>
    Task<StoredArtifact> CreateAsync(StoredArtifact artifact, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert die zuletzt angelegten Artefakte, absteigend nach Anlagezeit.
    /// </summary>
    /// <param name="limit">Maximale Anzahl zurückgegebener Datensätze.</param>
    Task<IReadOnlyList<StoredArtifact>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert das Artefakt mit der angegebenen Id oder <c>null</c>, wenn keines existiert.
    /// </summary>
    Task<StoredArtifact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
