// file: src/MultiPortUpload.Domain/Entities/BenchmarkRecord.cs

namespace MultiPortUpload.Domain.Entities;

/// <summary>
/// Persistierter Benchmark-Datensatz für einen einzelnen Upload-Vorgang.
/// Wird pro abgeschlossenem Upload erzeugt und dient der Auswertung der
/// Performance der verschiedenen Upload-Varianten (Adapter).
/// </summary>
public sealed class BenchmarkRecord
{
    /// <summary>Eindeutiger Primärschlüssel des Benchmark-Datensatzes.</summary>
    public Guid Id { get; init; }

    /// <summary>Artifact-Id des erzeugten/gespeicherten Uploads.</summary>
    public string ArtifactId { get; init; } = "";

    /// <summary>Verwendete Upload-Variante bzw. Adapter (z. B. "Local", "Streaming").</summary>
    public string UploadVariant { get; init; } = "";

    /// <summary>
    /// Name der Persona aus der Benchmark-Konfiguration (YAML), unter der dieser
    /// Upload ausgeführt wurde (z. B. "office_worker"). Null, wenn der Upload
    /// nicht im Rahmen eines Persona-Benchmarks erfolgte.
    /// </summary>
    public string? PersonaName { get; init; }

    /// <summary>
    /// Optionaler Verweis auf die zugehörige Benchmark-Run-Id, falls dieser Upload
    /// im Rahmen eines Persona-Benchmarks erfolgte.
    /// </summary>
    public Guid? BenchmarkRunId { get; set; }

    /// <summary>Ursprünglicher Dateiname des hochgeladenen Inhalts.</summary>
    public string OriginalFileName { get; init; } = "";

    /// <summary>Größe des hochgeladenen Inhalts in Bytes.</summary>
    public long SizeInBytes { get; init; }

    /// <summary>Zeitpunkt (UTC), zu dem der Upload gestartet wurde.</summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>Zeitpunkt (UTC), zu dem der Upload abgeschlossen war.</summary>
    public DateTimeOffset FinishedAtUtc { get; init; }

    /// <summary>Dauer des Uploads in Millisekunden.</summary>
    public long DurationInMilliseconds { get; init; }

    /// <summary>Berechneter Durchsatz in Megabyte pro Sekunde (0, wenn Dauer = 0).</summary>
    public double ThroughputMbPerSecond =>
        DurationInMilliseconds <= 0
            ? 0d
            : (SizeInBytes / 1024d / 1024d) / (DurationInMilliseconds / 1000d);

}
