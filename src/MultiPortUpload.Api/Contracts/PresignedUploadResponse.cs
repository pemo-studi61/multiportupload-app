// file: /src/MultiPortUpload.Api/Contracts/PresignedUploadResponse.cs

namespace MultiPortUpload.Api.Contracts;

public sealed record PresignedUploadResponse(
    string UploadUrl,
    string ArtifactId,
    string StoredFileName,
    string StoragePath
);