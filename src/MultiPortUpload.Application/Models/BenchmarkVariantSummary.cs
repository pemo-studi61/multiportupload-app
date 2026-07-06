// file: src/MultiPortUpload.Application/Models/BenchmarkVariantSummary.cs

namespace MultiPortUpload.Application.Models;

/// <summary>
/// Aggregierte Benchmark-Kennzahlen für eine Upload-Variante.
/// Wird von <see cref="Ports.Outbound.IBenchmarkRecordStore.GetSummaryAsync"/>
/// geliefert. Settable Properties, damit EF Core das Ergebnis einer rohen
/// SQL-Abfrage materialisieren kann.
/// </summary>
public sealed class BenchmarkVariantSummary
{
    public string UploadVariant { get; set; } = "";

    /// <summary>Anzahl erfasster Uploads dieser Variante.</summary>
    public long UploadCount { get; set; }

    /// <summary>Durchschnittliche Upload-Dauer in Millisekunden.</summary>
    public double AverageDurationMs { get; set; }

    /// <summary>Kürzeste Upload-Dauer in Millisekunden.</summary>
    public long MinDurationMs { get; set; }

    /// <summary>Längste Upload-Dauer in Millisekunden.</summary>
    public long MaxDurationMs { get; set; }

    /// <summary>95. Perzentil der Upload-Dauer in Millisekunden.</summary>
    public double P95DurationMs { get; set; }

    /// <summary>Summe aller hochgeladenen Bytes dieser Variante.</summary>
    public long TotalBytes { get; set; }
}
