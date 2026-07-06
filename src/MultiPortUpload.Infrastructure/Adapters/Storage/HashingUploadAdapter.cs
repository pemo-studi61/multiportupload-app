// file: src/MultiPortUpload.Infrastructure/Storage/HashingUploadAdapter.cs

using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Application.Logging;
using MultiPortUpload.Infrastructure.Options;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class HashingUploadAdapter : IUploadAdapter, IUploadPort
{
    private readonly LocalStorageOptions _options;
    private readonly ILogger<HashingUploadAdapter> _logger;

    // Über Variant wird ein Adapter ausgewählt
    public string Variant => "Hashing";

    public string Description => "Speichert die Datei und berechnet dabei einen SHA-256-Hash zur Integritätsprüfung.";

    // Konstruktor für Initialisierung der Felder
    public HashingUploadAdapter(
        IOptions<LocalStorageOptions> options,
        ILogger<HashingUploadAdapter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    // Die zentrale Methode für den Upload
    public async Task<UploadPortResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        // Verzeichnis anlegen
        // TODO: try/catch?
        Directory.CreateDirectory(_options.RootPath);

        var artifactId = Guid.NewGuid().ToString();
        var storedFileName = $"{artifactId}_{fileName}";
        var storagePath = Path.Combine(_options.RootPath, storedFileName);
        long sizeInBytes = 0;
        string hash;

        using var sha256 = SHA256.Create();

        try {
            await using var fileStream = new FileStream(
                storagePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            // TODO: buffer größe variabel?
            var buffer = new byte[81920];
            int bytesRead;
            
            sizeInBytes = 0;
            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                // Schreiben
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                // Hash berechnen
                sha256.TransformBlock(buffer, 0, bytesRead, null, 0);

                sizeInBytes += bytesRead;
            }

            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

            hash = Convert.ToHexString(sha256.Hash!);

            _logger.LogInformation(
                "Hashing upload finished: {StoredFileName}, Size={SizeInBytes}, SHA256={Hash}",
                storedFileName,
                sizeInBytes,
                hash);

        } catch (SystemException ex) {
            _logger.LogError(
                ex,
                "Fehler in HashingUpload->UploadAsync: {StoragePath}", storagePath);

            try { if (File.Exists(storagePath)) File.Delete(storagePath); }
            catch (Exception cleanupEx)
            {
                _logger.LogError(UploadEventIds.UploadCleanupFailed, cleanupEx,
                    "Aufräumen der Teildatei {StoragePath} fehlgeschlagen", storagePath);
            }

            // Fehler weiterreichen – sonst würde der Upload fälschlich als
            // erfolgreich (ggf. mit unvollständiger Größe) aufgezeichnet.
            throw;
        }

        // Rückgabe immer als einheitlicher UploadPortResult
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