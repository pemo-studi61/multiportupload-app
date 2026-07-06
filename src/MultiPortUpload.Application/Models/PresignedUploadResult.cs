// file: src/MultiPortUpload.Application/Models/PresignedUploadResult.cs  

namespace MultiPortUpload.Application.Models;

public sealed record PresignedUploadResult(
    string UploadUrl,
    string ArtifactId,
    string StoredFileName,
    string StoragePath);