// file: src/MultiPortUpload.Application/Abstractions/IUploadPort.cs

namespace MultiPortUpload.Application.Abstractions;

public interface IUploadPort
{
    string Variant { get; }
    Task<UploadPortResult> UploadAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);
}

