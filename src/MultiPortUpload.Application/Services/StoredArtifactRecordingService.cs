// file: src/MultiPortUpload.Application/Services/StoredArtifactRecordingService.cs

using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MultiPortUpload.Application.Services;

/// <summary>
/// Zentrale, resiliente Persistenz von Datei-Metadaten (<see cref="StoredArtifact"/>)
/// in der Tabelle <c>stored_artifacts</c>. Wird von den Upload-Pfaden genutzt, damit
/// pro erfolgreichem Upload genau ein Metadaten-Datensatz entsteht. Persistenzfehler
/// werden nur protokolliert und nie an den Aufrufer weitergereicht.
/// </summary>
public sealed class StoredArtifactRecordingService
{
    private readonly IStoredArtifactStore _store;
    private readonly ILogger<StoredArtifactRecordingService> _logger;

    public StoredArtifactRecordingService(
        IStoredArtifactStore store,
        ILogger<StoredArtifactRecordingService> logger)
    {
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Persistiert die Datei-Metadaten und liefert die von der Datenbank generierte
    /// Artefakt-Id (<c>gen_random_uuid()</c>) zurück, oder <c>null</c>, wenn die
    /// Persistenz fehlschlug bzw. deaktiviert ist.
    /// </summary>
    public async Task<Guid?> RecordAsync(
        string originalFileName,
        string storedFileName,
        string mimeType,
        long sizeInBytes,
        string? sha256,
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var artifact = new StoredArtifact
            {
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                MimeType = string.IsNullOrWhiteSpace(mimeType)
                    ? "application/octet-stream"
                    : mimeType,
                FileExtension = Path.GetExtension(originalFileName),
                SizeInBytes = sizeInBytes,
                Sha256 = sha256 ?? string.Empty,
                StoragePath = storagePath
            };

            var stored = await _store.CreateAsync(artifact, cancellationToken);

            return stored.Id == Guid.Empty ? null : stored.Id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Datei-Metadaten für {FileName} konnten nicht gespeichert werden.",
                originalFileName);

            return null;
        }
    }
}
