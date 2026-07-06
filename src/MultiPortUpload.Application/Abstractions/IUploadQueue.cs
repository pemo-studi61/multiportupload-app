// file: src/MultiPortUpload.Application/Abstractions/IUploadQueue.cs

using MultiPortUpload.Application.Models;

public interface IUploadQueue
{
    ValueTask EnqueueAsync(UploadQueueItem item, CancellationToken cancellationToken);
    ValueTask<UploadQueueItem> DequeueAsync(CancellationToken cancellationToken);
}