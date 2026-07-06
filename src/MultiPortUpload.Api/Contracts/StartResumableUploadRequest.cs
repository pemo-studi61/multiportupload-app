// file: src/MultiPortUpload.Api/Contracts/StartResumableUploadRequest.cs

namespace MultiPortUpload.Api.Contracts.Resumable;

public sealed class StartResumableUploadRequest
{
    public string FileName { get; init; } = string.Empty;

    public long SizeInBytes { get; init; }
}