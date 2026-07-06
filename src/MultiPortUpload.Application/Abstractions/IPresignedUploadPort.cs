// file: /src/MultiPortUpload.Application/Abstractions/IPresignedUploadPort.cs 

namespace MultiPortUpload.Application.Abstractions;

using MultiPortUpload.Application.Models;

public interface IPresignedUploadPort
{
    Task<PresignedUploadResult> CreateUploadUrlAsync(
        PresignedUploadCommand command,
        CancellationToken cancellationToken = default);
}