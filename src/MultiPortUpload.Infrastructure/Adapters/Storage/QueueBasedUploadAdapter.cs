// file: src/MultiPortUpload.Infrastructure/Storage/QueueBasedUploadAdapter.cs

using System.Security.Cryptography;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Application.Models;
using MultiPortUpload.Application.Logging;
using MultiPortUpload.Infrastructure.Options;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class QueueBasedUploadAdapter : IUploadAdapter, IUploadPort
{
    private readonly IUploadQueue uploadQueue;
    private readonly LocalStorageOptions _options;
    private readonly ILogger<QueueBasedUploadAdapter> _logger;


    public QueueBasedUploadAdapter(
        IUploadQueue uploadQueue,
        IOptions<LocalStorageOptions> options,
        ILogger<QueueBasedUploadAdapter> logger)
    {
        this.uploadQueue = uploadQueue;
        this._options = options.Value;
        this._logger = logger;
    }

    public string Variant => "QueueBased";

    public string Description => "Nimmt die Datei entgegen und verarbeitet den Upload asynchron über eine Warteschlange.";

    public async Task<UploadPortResult> UploadAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var artifactId = Guid.NewGuid().ToString("N");
        var safeFileName = Path.GetFileName(originalFileName);
        var storedFileName = $"{artifactId}_{safeFileName}";
        var fullPath = Path.Combine(this._options.RootPath, storedFileName);
        string hash;

        try
        {
            using var sha256 = SHA256.Create();
            await using (var fileStream = File.Create(fullPath))
            await using (var cryptoStream = new CryptoStream(
                fileStream, sha256, CryptoStreamMode.Write, leaveOpen: true))
            {
                await content.CopyToAsync(cryptoStream, cancellationToken);
                await cryptoStream.FlushFinalBlockAsync(cancellationToken);
            }

            hash = Convert.ToHexString(sha256.Hash!);

            await uploadQueue.EnqueueAsync(
                new UploadQueueItem(
                    artifactId,
                    fullPath,
                    safeFileName,
                    Variant,
                    DateTime.UtcNow),
                cancellationToken);
            _logger.LogInformation(
                "QueueBasedUpload->UploadAsync: Neues Item in die Queue eingereiht"
            );
        }
        catch (SystemException ex)
        {
            _logger.LogError(
                UploadEventIds.QueueBasedUploadFailed,
                ex,
                "QueueBasedUpload->UploadAsync: Allgemeiner Fehler"
            );

            try { if (File.Exists(fullPath)) File.Delete(fullPath); }
            catch (Exception cleanupEx)
            {
                _logger.LogError(UploadEventIds.UploadCleanupFailed, cleanupEx,
                    "Aufräumen der Teildatei {FullPath} fehlgeschlagen", fullPath);
            }

            // Fehler weiterreichen – sonst würde der Upload fälschlich als
            // erfolgreich aufgezeichnet (und FileInfo.Length unten würde werfen).
            throw;
        }

        var fileInfo = new FileInfo(fullPath);
        return new UploadPortResult(
            artifactId,
            safeFileName,
            fullPath,
            fileInfo.Length,
            Variant,
            hash);
    }

    public Task<UploadPortResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}