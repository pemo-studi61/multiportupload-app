// file: src/MultiPortUpload.Application/Models/ResumableUploadSession.cs

namespace MultiPortUpload.Application.Models;

public sealed class ResumableUploadSession
{
    public string UploadId { get; init; } = Guid.NewGuid().ToString("N");

    public string OriginalFileName { get; init; } = string.Empty;

    public long SizeInBytes { get; init; }

    public int ChunkSizeBytes { get; init; }

    public int TotalChunks { get; init; }

    public List<int> ReceivedChunks { get; init; } = new();

    public DateTime StartedAtUtc { get; init; } = DateTime.UtcNow;
}