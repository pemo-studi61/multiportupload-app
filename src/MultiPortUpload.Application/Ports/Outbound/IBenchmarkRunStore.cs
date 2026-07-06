// file: src/MultiPortUpload.Application/Ports/Outbound/IBenchmarkRunStore.cs

using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Application.Ports.Outbound;

/// <summary>
/// Outbound-Port zum Anlegen und Abfragen von Benchmark-Läufen
/// (<see cref="BenchmarkRun"/>). Die konkrete Implementierung (PostgreSQL via
/// EF Core) liegt in der Infrastructure-Schicht.
/// </summary>
public interface IBenchmarkRunStore
{
    /// <summary>
    /// Legt einen neuen Benchmark-Lauf an und liefert ihn inklusive der von der
    /// Datenbank generierten Werte (Id, CreatedAt, RunUuid) zurück.
    /// </summary>
    Task<BenchmarkRun> CreateAsync(BenchmarkRun run, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert die zuletzt angelegten Benchmark-Läufe, absteigend nach Anlagezeit.
    /// </summary>
    /// <param name="limit">Maximale Anzahl zurückgegebener Läufe.</param>
    Task<IReadOnlyList<BenchmarkRun>> GetRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert den Benchmark-Lauf mit der angegebenen Id oder <c>null</c>, wenn keiner existiert.
    /// </summary>
    Task<BenchmarkRun?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Löscht den Benchmark-Lauf mit der angegebenen Id.
    /// </summary>
    /// <returns><c>true</c>, wenn ein Lauf gelöscht wurde; <c>false</c>, wenn keiner existierte.</returns>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
