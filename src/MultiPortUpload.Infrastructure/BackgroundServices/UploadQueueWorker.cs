// file: src/MultiPortUpload.Infrastructure/BackgroundServices/UploadQueueWorker.cs

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Models;

namespace MultiPortUpload.Infrastructure.BackgroundServices;

public sealed class UploadQueueWorker : BackgroundService
{
    private readonly IUploadQueue uploadQueue;

    private readonly ILogger<UploadQueueWorker> logger;

    public UploadQueueWorker(
        IUploadQueue uploadQueue,
        ILogger<UploadQueueWorker> logger)
    {
        this.uploadQueue = uploadQueue;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "UploadQueueWorker gestartet");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var item = await uploadQueue.DequeueAsync(
                    stoppingToken);

                logger.LogInformation(
                    "Queue-Upload verarbeitet: {UploadId} Datei={FileName}",
                    item.UploadId,
                    item.OriginalFileName);

                await Task.Delay(
                    1000,
                    stoppingToken);

                logger.LogInformation(
                    "Queue-Upload abgeschlossen: {UploadId}",
                    item.UploadId);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation(
                    "UploadQueueWorker beendet");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Fehler im UploadQueueWorker");
            }
        }
    }
}