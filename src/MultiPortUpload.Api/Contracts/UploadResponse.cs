// file: src/MultiPortUpload.Api/Contracts/UploadResponse.cs

namespace MultiPortUpload.Api.Contracts;

public sealed record UploadResponse(
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