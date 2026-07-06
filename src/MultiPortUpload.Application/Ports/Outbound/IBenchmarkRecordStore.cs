// file: src/MultiPortUpload.Application/Ports/Outbound/IBenchmarkRecordStore.cs

using MultiPortUpload.Application.Models;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Application.Ports.Outbound;

/// <summary>
/// Outbound-Port zum Persistieren und Abfragen von Benchmark-Datensätzen.
/// Die konkrete Implementierung (z. B. PostgreSQL via EF Core) liegt in der
/// Infrastructure-Schicht, sodass die Anwendungsschicht unabhängig von der
/// gewählten Persistenztechnologie bleibt.
/// </summary>
public interface IBenchmarkRecordStore
{
    /// <summary>Speichert einen einzelnen Benchmark-Datensatz.</summary>
    Task AddAsync(BenchmarkRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert die zuletzt erfassten Benchmark-Datensätze, absteigend nach Startzeit.
    /// </summary>
    /// <param name="limit">Maximale Anzahl zurückgegebener Datensätze.</param>
    /// <param name="variant">Optionaler Filter auf eine Upload-Variante.</param>
    Task<IReadOnlyList<BenchmarkRecord>> GetRecentAsync(
        int limit = 100,
        string? variant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert alle Benchmark-Datensätze, die zum Benchmark-Lauf mit der angegebenen
    /// RunUuid gehören, aufsteigend nach Startzeit.
    /// </summary>
    Task<IReadOnlyList<BenchmarkRecord>> GetByRunUuidAsync(
        Guid runUuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert aggregierte Kennzahlen (Anzahl, Dauer-Statistiken, p95, Bytes) je
    /// Upload-Variante.
    /// </summary>
    Task<IReadOnlyList<BenchmarkVariantSummary>> GetSummaryAsync(
        CancellationToken cancellationToken = default);
}
