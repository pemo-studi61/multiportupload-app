// file: src/MultiPortUpload.Infrastructure/Queue/InMemoryUploadQueue.cs
using System.Threading.Channels;

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Models;

namespace MultiPortUpload.Infrastructure.Queue;

public sealed class InMemoryUploadQueue : IUploadQueue
{
    private readonly Channel<UploadQueueItem> channel =
        Channel.CreateUnbounded<UploadQueueItem>();

    public async ValueTask EnqueueAsync(
        UploadQueueItem item,
        CancellationToken cancellationToken)
    {
        await channel.Writer.WriteAsync(
            item,
            cancellationToken);
    }

    public async ValueTask<UploadQueueItem> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return await channel.Reader.ReadAsync(
            cancellationToken);
    }
}