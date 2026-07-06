// file: src/MultiPortUpload.Application/Abstractions/IChunkedUploadPort.cs

namespace MultiPortUpload.Application.Abstractions;

public interface IChunkedUploadPort
{
    Task<ChunkUploadResult> UploadChunkAsync(
        Stream stream,
        string uploadId,
        string originalFileName,
        int chunkIndex,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<UploadPortResult> CompleteAsync(
        string uploadId,
        string originalFileName,
        int totalChunks,
        CancellationToken cancellationToken);
}

public sealed record ChunkUploadResult(
    string UploadId,
    string OriginalFileName,
    int ChunkIndex,
    int TotalChunks,
    bool IsComplete);