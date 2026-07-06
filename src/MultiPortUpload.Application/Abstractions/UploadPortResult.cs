// file: src/MultiPortUpload.Application/Abstractions/UploadPortResult.cs

namespace MultiPortUpload.Application.Abstractions;

public sealed record UploadPortResult(
    string ArtifactId,
    string StoredFileName,
    string StoragePath,
    long SizeInBytes,
    string UploadVariant,
    // SHA256-Prüfsumme (Hex, Großbuchstaben) der gespeicherten Bytes für den
    // Integritätsabgleich. Null, falls der Adapter keine Prüfsumme berechnet.
    string? Sha256 = null);


