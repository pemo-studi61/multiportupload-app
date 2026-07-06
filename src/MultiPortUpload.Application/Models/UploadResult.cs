// file: src/MultiPortUpload.Application/Models/UploadResult.cs

namespace MultiPortUpload.Application.Models;

public sealed record UploadResult(
    string ArtifactId,
    string OriginalFileName,
    string StoredFileName,
    string StoragePath,
    string UploadVariant,
    long SizeInBytes,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset FinishedAtUtc,
    long DurationInMilliseconds,
    string? Sha256 = null);