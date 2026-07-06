// file: src/MultiPortUpload.Domain/Entities/BenchmarkRun.cs

namespace MultiPortUpload.Domain.Entities;

/// <summary>
/// Ein Benchmark-Lauf gruppiert mehrere einzelne <see cref="BenchmarkRecord"/>
/// (einen pro Upload) zu einem benannten Durchlauf. Wird vor dem eigentlichen
/// Benchmark angelegt; die zugehörigen Datensätze verweisen anschließend über
/// ihre BenchmarkRunId auf diesen Lauf.
/// </summary>
public sealed class BenchmarkRun
{
    /// <summary>Datenbankgenerierter Primärschlüssel (bigint identity).</summary>
    public long Id { get; init; }

    /// <summary>Zeitpunkt (UTC), zu dem der Lauf angelegt wurde. Von der DB gesetzt (now()).</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Optionale, frei wählbare Beschreibung des Laufs.</summary>
    public string? Description { get; init; }

    /// <summary>Optionaler Name des Runners/der Maschine, die den Lauf ausgeführt hat.</summary>
    public string? RunnerName { get; init; }

    /// <summary>Optionale Gesamtlaufzeit des Benchmarks in Sekunden.</summary>
    public float? RunningTime { get; init; }

    /// <summary>Optionaler Standort, an dem der Lauf stattgefunden hat.</summary>
    public string? Location { get; init; }

    /// <summary>Optionaler, frei wählbarer Kommentar zum Lauf.</summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Stabile, fachliche Kennung des Laufs (UUID). Von der DB generiert
    /// (gen_random_uuid()) und dient als externer Verweis für die zugehörigen
    /// Benchmark-Datensätze.
    /// </summary>
    public Guid RunUuid { get; init; }
}
