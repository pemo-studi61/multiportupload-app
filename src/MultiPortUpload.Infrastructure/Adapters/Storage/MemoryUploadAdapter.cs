// file: src/MultiPortUpload.Infrastructure/Storage/MemoryUploadAdapter.cs

using System.Security.Cryptography;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Application.Logging;
using MultiPortUpload.Infrastructure.Options;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class MemoryUploadAdapter : IUploadAdapter, IUploadPort
{
    private readonly LocalStorageOptions _options;
    private readonly ILogger<MemoryUploadAdapter> _logger;

    public string Variant => "Memory";

    public string Description => "Hält die Datei vollständig im Arbeitsspeicher (primär für Tests und Benchmarks).";

    public MemoryUploadAdapter(
        IOptions<LocalStorageOptions> options,
        ILogger<MemoryUploadAdapter> logger)
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
        var sizeInBytes = 0L;
        string hash;

        try {
            await using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream, cancellationToken);

            memoryStream.Position = 0;

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
                await memoryStream.CopyToAsync(cryptoStream, cancellationToken);
                await cryptoStream.FlushFinalBlockAsync(cancellationToken);
            }

            hash = Convert.ToHexString(sha256.Hash!);

            sizeInBytes = memoryStream.Length;

            _logger.LogInformation(
                "Memory upload finished: {StoredFileName} ({SizeInBytes} bytes)",
                storedFileName,
                sizeInBytes);
        } catch (SystemException ex) {
            _logger.LogError(
                UploadEventIds.MemoryUploadFailed,
                ex,
                "Fehler in MemoryUpload->UploadAsync");

            try { if (File.Exists(storagePath)) File.Delete(storagePath); }
            catch (Exception cleanupEx)
            {
                _logger.LogError(UploadEventIds.UploadCleanupFailed, cleanupEx,
                    "Aufräumen der Teildatei {StoragePath} fehlgeschlagen", storagePath);
            }

            // Fehler weiterreichen – sonst würde der Upload fälschlich als
            // erfolgreich (ggf. mit 0 Bytes) aufgezeichnet.
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