// file: src/MultiPortUpload.Domain/Entities/StoredArtifact.cs

namespace MultiPortUpload.Domain.Entities;

/// <summary>
/// Persistierte Metadaten zu einer hochgeladenen Datei. Pro erfolgreichem Upload
/// wird genau ein Datensatz in der Tabelle <c>stored_artifacts</c> angelegt. Die
/// eigentlichen Dateiinhalte liegen weiterhin im jeweiligen Speicher-Backend
/// (lokal, S3/MinIO, …); diese Entität hält nur die beschreibenden Metadaten.
/// </summary>
public sealed class StoredArtifact
{
    /// <summary>Primärschlüssel (UUID). Wird von der Datenbank via gen_random_uuid() vergeben.</summary>
    public Guid Id { get; init; }

    /// <summary>Ursprünglicher, vom Client übermittelter Dateiname.</summary>
    public string OriginalFileName { get; init; } = string.Empty;

    /// <summary>Im Speicher-Backend abgelegter (ggf. umbenannter) Dateiname.</summary>
    public string StoredFileName { get; init; } = string.Empty;

    /// <summary>MIME-Typ der Datei (z. B. <c>image/png</c>).</summary>
    public string MimeType { get; init; } = string.Empty;

    /// <summary>Dateiendung inkl. Punkt (z. B. <c>.png</c>); leer, wenn keine vorhanden.</summary>
    public string FileExtension { get; init; } = string.Empty;

    /// <summary>Dateigröße in Bytes.</summary>
    public long SizeInBytes { get; init; }

    /// <summary>SHA-256-Hash der Datei (Hex). Leer, wenn der Adapter keinen berechnet hat.</summary>
    public string Sha256 { get; init; } = string.Empty;

    /// <summary>Speicherort/-schlüssel im jeweiligen Backend.</summary>
    public string StoragePath { get; init; } = string.Empty;

    /// <summary>Zeitpunkt (UTC) des Uploads. Von der DB gesetzt (now()).</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }
}
