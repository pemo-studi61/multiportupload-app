// file: src/MultiPortUpload.Api/Contracts/StartResumableUploadResponse.cs

namespace MultiPortUpload.Api.Contracts.Resumable;

public sealed class StartResumableUploadResponse
{
    public string UploadId { get; init; } = string.Empty;

    public int ChunkSizeBytes { get; init; }

    public int TotalChunks { get; init; }
}