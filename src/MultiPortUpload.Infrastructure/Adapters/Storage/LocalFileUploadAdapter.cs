// file: src/MultiPortUpload.Infrastructure/Adapters/Storage/LocalFileUploadAdapter.cs
// PM: 11/05/2026 - Ausführliches Logging mit EventId und Fehlerbehandlung

using System.Globalization;
using System.Security.Cryptography;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MultiPortUpload.Application.Logging;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class LocalFileUploadAdapter : IUploadAdapter, IUploadPort
{
    private readonly LocalStorageOptions _options;
    private readonly ILogger<LocalFileUploadAdapter> _logger;

    public string Variant => "LocalFile";

    public string Description => "Speichert die Datei direkt im lokalen Dateisystem (uploads-Verzeichnis).";

    public LocalFileUploadAdapter(
        IOptions<LocalStorageOptions> options,
        ILogger<LocalFileUploadAdapter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UploadPortResult> UploadAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        Directory.CreateDirectory(_options.RootPath);

        var artifactId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        var safeOriginalFileName = Path.GetFileName(originalFileName);
        var storedFileName = $"{artifactId}_{safeOriginalFileName}";
        var fullPath = Path.Combine(_options.RootPath, storedFileName);
        var fileInfo = new FileInfo(fullPath);
        var storedFileSize = 0L;
        string hash;

        try
        {
            if (File.Exists(fullPath))
            {
                _logger.LogWarning(
                    UploadEventIds.UploadConflictDetected,
                    "Upload conflict: File {FullPath} already exists. Generating new artifact ID.",
                    fullPath);

                artifactId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                storedFileName = $"{artifactId}_{safeOriginalFileName}";
                fullPath = Path.Combine(_options.RootPath, storedFileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                UploadEventIds.UploadConflictCheckFailed,
                ex,
                "Failed to check for existing file {FullPath}. Upload cannot proceed.",
                fullPath);

            throw new IOException($"Failed to check for existing file {fullPath}.", ex);
        }

        try
        {
            await using var targetStream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            _logger.LogInformation(
                UploadEventIds.UploadStarted,
                "Upload started. Adapter={Adapter}, FileName={FileName}, SizeInBytes={SizeInBytes}",
                Variant,
                fullPath,
                fileInfo.Length);

            using var sha256 = SHA256.Create();
            await using (var cryptoStream = new CryptoStream(
                targetStream, sha256, CryptoStreamMode.Write, leaveOpen: true))
            {
                await content.CopyToAsync(cryptoStream, cancellationToken);
                await cryptoStream.FlushFinalBlockAsync(cancellationToken);
            }

            hash = Convert.ToHexString(sha256.Hash!);

            await targetStream.FlushAsync(cancellationToken);
            // Größe erneut ermitteln, um sicherzustellen, dass die Datei vollständig geschrieben wurde
            storedFileSize = new FileInfo(fullPath).Length;

            _logger.LogInformation(
                UploadEventIds.UploadCompleted,
                "File stored locally at {FullPath} ({SizeInBytes} bytes)",
                fullPath,
                storedFileSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                UploadEventIds.UploadFailed,
                ex,
                "Upload failed for file {FullPath}. Cleaning up any partially written file.",
                fullPath);

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation(
                        UploadEventIds.UploadCleanupSuccess,
                        "Successfully cleaned up partial file {FullPath} after upload failure.",
                        fullPath);
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(
                    UploadEventIds.UploadCleanupFailed,
                    cleanupEx,
                    "Failed to clean up partial file {FullPath} after upload failure. Manual cleanup may be required.",
                    fullPath);
            }

            throw new IOException($"Upload failed for file {fullPath}.", ex);
        }

        return new UploadPortResult(
            artifactId,
            storedFileName,
            fullPath,
            storedFileSize,
            "LocalFile",
            hash);
    }

    public Task<UploadPortResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}