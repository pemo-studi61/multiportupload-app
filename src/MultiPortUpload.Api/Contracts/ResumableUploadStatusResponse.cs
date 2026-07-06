// file: src/MultiPortUpload.Api/Contracts/ResumableUploadStatusResponse.cs

public sealed class ResumableUploadStatusResponse
{
    public string UploadId { get; init; } = string.Empty;

    public int TotalChunks { get; init; }

    public IReadOnlyList<int> ReceivedChunks { get; init; }
        = Array.Empty<int>();

    public IReadOnlyList<int> MissingChunks { get; init; }
        = Array.Empty<int>();
}