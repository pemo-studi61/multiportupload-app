// file: src/MultiPortUpload.Infrastructure/Adapters/Storage/StreamingUploadAdapter.cs

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MultiPortUpload.Application.Logging;
using System.Diagnostics;
using System.Security.Cryptography;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class StreamingUploadAdapter : IUploadAdapter, IUploadPort
{
    private readonly LocalStorageOptions _options;
    private readonly ILogger<StreamingUploadAdapter> _logger;

    public string Variant => "Streaming";

    public string Description => "Streamt die Datei blockweise auf die Festplatte, ohne sie komplett im Arbeitsspeicher zu halten.";

    public StreamingUploadAdapter(
        IOptions<LocalStorageOptions> options,
        ILogger<StreamingUploadAdapter> logger)
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
        var stopwatch = Stopwatch.StartNew();
        Directory.CreateDirectory(_options.RootPath);
        var filePath = Path.Combine(_options.RootPath, fileName);
        long sizeInBytes = 0;
        string hash;

        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        try
        {
            await using var fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            var buffer = new byte[81920];
            int bytesRead;

            

            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                sha256.AppendData(buffer, 0, bytesRead);
                sizeInBytes += bytesRead;
            }

            hash = Convert.ToHexString(sha256.GetHashAndReset());

            stopwatch.Stop();

            _logger.LogInformation(
                UploadEventIds.StreamingUploadCompleted,
                "Streaming upload finished: {FileName} ({ContentType}, {SizeInBytes} bytes) in {DurationMs} ms",
                fileName,
                contentType,
                sizeInBytes,
                stopwatch.ElapsedMilliseconds);
        }
        catch (SystemException ex)
        {
            _logger.LogError(
                UploadEventIds.StreamingUploadFailed,
                ex,
                "StreamingUploadAdapter->UploadAsync: Fehler beim Schreiben von {FileName}",
                fileName
            );

            // Teildatei aufräumen und den Fehler weiterreichen – sonst würde der
            // Upload fälschlich als erfolgreich (ggf. mit 0 Bytes) gemeldet.
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(
                    UploadEventIds.StreamingUploadFailed,
                    cleanupEx,
                    "StreamingUploadAdapter->UploadAsync: Aufräumen der Teildatei {FilePath} fehlgeschlagen",
                    filePath
                );
            }

            throw;
        }

        var artifactId = Guid.NewGuid().ToString();
        var storedFileName = $"{artifactId}_{fileName}";

        return new UploadPortResult(
            artifactId,
            storedFileName,
            filePath,
            sizeInBytes,
            Variant,
            hash);
    }

    public Task<UploadPortResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}