// file: src/MultiPortUpload.Infrastructure/Storage/ChunkedUploadAdapter.cs

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Application.Logging;
using MultiPortUpload.Infrastructure.Options;
using System.Security.Cryptography;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class ChunkedUploadAdapter : IUploadAdapter, IChunkedUploadPort
{
    private readonly LocalStorageOptions _options;
    private readonly ILogger<ChunkedUploadAdapter> _logger;

    public string Variant => "Chunked";

    public string Description => "Nimmt die Datei in mehreren Chunks entgegen und setzt sie serverseitig wieder zusammen.";

    public ChunkedUploadAdapter(
        IOptions<LocalStorageOptions> options,
        ILogger<ChunkedUploadAdapter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ChunkUploadResult> UploadChunkAsync(
        Stream stream,
        string uploadId,
        string originalFileName,
        int chunkIndex,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        var chunkDirectory = Path.Combine(_options.RootPath, "chunks", uploadId);
        Directory.CreateDirectory(chunkDirectory);

        var chunkPath = Path.Combine(chunkDirectory, $"{chunkIndex:D6}.part");

        try {
            await using var fileStream = new FileStream(
                chunkPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await stream.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation(
                UploadEventIds.ChunkedUploadCompleted,
                "Chunk uploaded: {UploadId}, {ChunkIndex}/{TotalChunks}, {OriginalFileName}",
                uploadId,
                chunkIndex,
                totalChunks,
                originalFileName);
        } catch (SystemException ex) {
            _logger.LogError(
                UploadEventIds.ChunkedUploadFailed,
                ex,
                "ChunkedUpload->UploadChunkAsync: Allgemeiner Fehler");
        }

        var uploadedChunkCount = Directory
            .GetFiles(chunkDirectory, "*.part")
            .Length;

        return new ChunkUploadResult(
            uploadId,
            originalFileName,
            chunkIndex,
            totalChunks,
            uploadedChunkCount == totalChunks);
    }

    public async Task<UploadPortResult> CompleteAsync(
        string uploadId,
        string originalFileName,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        var chunkDirectory = Path.Combine(_options.RootPath, "chunks", uploadId);

        if (!Directory.Exists(chunkDirectory))
        {
            var logMsg = $"Chunk-Verzeichnis nicht gefunden: {chunkDirectory}";
            _logger.LogError(logMsg);
            throw new DirectoryNotFoundException(logMsg);
        }

        var artifactId = Guid.NewGuid().ToString();
        var storedFileName = $"{artifactId}_{originalFileName}";
        var targetPath = Path.Combine(_options.RootPath, storedFileName);

        await using var targetStream = new FileStream(
            targetPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        using var sha256 = SHA256.Create();
        string hash;

        await using (var cryptoStream = new CryptoStream(
            targetStream, sha256, CryptoStreamMode.Write, leaveOpen: true))
        {
            for (var chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                var chunkPath = Path.Combine(chunkDirectory, $"{chunkIndex:D6}.part");

                if (!File.Exists(chunkPath))
                {
                    throw new FileNotFoundException(
                        $"Chunk fehlt: {chunkIndex}",
                        chunkPath);
                }

                await using var chunkStream = new FileStream(
                    chunkPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 81920,
                    useAsync: true);

                await chunkStream.CopyToAsync(cryptoStream, cancellationToken);
            }

            await cryptoStream.FlushFinalBlockAsync(cancellationToken);
        }

        hash = Convert.ToHexString(sha256.Hash!);

        Directory.Delete(chunkDirectory, recursive: true);

        var sizeInBytes = new FileInfo(targetPath).Length;

        _logger.LogInformation(
            "Chunked upload completed: {UploadId}, {StoredFileName}, {SizeInBytes} bytes",
            uploadId,
            storedFileName,
            sizeInBytes);

        return new UploadPortResult(
            artifactId,
            storedFileName,
            targetPath,
            sizeInBytes,
            Variant,
            hash);
    }

    public Task<UploadPortResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}