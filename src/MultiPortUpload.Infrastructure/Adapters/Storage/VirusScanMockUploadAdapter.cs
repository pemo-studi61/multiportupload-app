// file: src/MultiPortUpload.Infrastructure/Storage/VirusScanMockUploadAdapter.cs

using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Application.Logging;
using MultiPortUpload.Infrastructure.Options;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class VirusScanMockUploadAdapter : IUploadAdapter, IUploadPort
{
    private readonly LocalStorageOptions _options;
    private readonly ILogger<VirusScanMockUploadAdapter> _logger;

    public string Variant => "VirusScanMock";

    public string Description => "Simuliert einen Virenscan vor dem Speichern (Mock, ohne echten Scanner).";

    public VirusScanMockUploadAdapter(
        IOptions<LocalStorageOptions> options,
        ILogger<VirusScanMockUploadAdapter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UploadPortResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.RootPath);

        var artifactId = Guid.NewGuid().ToString();
        var storedFileName = $"{artifactId}_{fileName}";
        var storagePath = Path.Combine(_options.RootPath, storedFileName);
        var sizeInBytes  = 0L;
        string hash;
        try
        {
            await using var fileStream = new FileStream(
                storagePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            using var sha256 = SHA256.Create();
            await using (var cryptoStream = new CryptoStream(
                fileStream, sha256, CryptoStreamMode.Write, leaveOpen: true))
            {
                await stream.CopyToAsync(cryptoStream, cancellationToken);
                await cryptoStream.FlushFinalBlockAsync(cancellationToken);
            }

            hash = Convert.ToHexString(sha256.Hash!);

            sizeInBytes = new FileInfo(storagePath).Length;

            // Simulierter Virenscan
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);

            _logger.LogInformation(
                UploadEventIds.ViruscanUploadCompleted,
                "Virus scan mock upload finished: {StoredFileName}, Size={SizeInBytes}, ScanResult={ScanResult}",
                storedFileName,
                sizeInBytes,
                "clean");
        } catch (SystemException ex) {
            _logger.LogError(
                UploadEventIds.ViruscanUploadFailed,
                ex,
                "VirusScanMockUploadAdapter->UploadAsync: Allgemeiner Fehler"
            );

            try { if (File.Exists(storagePath)) File.Delete(storagePath); }
            catch (Exception cleanupEx)
            {
                _logger.LogError(UploadEventIds.UploadCleanupFailed, cleanupEx,
                    "Aufräumen der Teildatei {StoragePath} fehlgeschlagen", storagePath);
            }

            // Fehler weiterreichen – sonst würde der Upload fälschlich als
            // erfolgreich aufgezeichnet.
            throw;
        }
        return new UploadPortResult(
            artifactId,
            storedFileName,
            storagePath,
            sizeInBytes,
            Variant,
            hash);
    }

    public Task<UploadPortResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}