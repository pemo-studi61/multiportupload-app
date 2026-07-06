// file: src/MultiPortUpload.Application/Services/BenchmarkRecordingService.cs

using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MultiPortUpload.Application.Services;

/// <summary>
/// Zentrale, resiliente Persistenz von Benchmark-Datensätzen. Wird von allen
/// Upload-Pfaden (UploadService, Chunked-Upload) genutzt, damit jeder Upload an
/// einer Stelle einheitlich erfasst wird. Persistenzfehler werden nur
/// protokolliert und nie an den Aufrufer weitergereicht.
/// </summary>
public sealed class BenchmarkRecordingService
{
    private readonly IBenchmarkRecordStore _store;
    private readonly ILogger<BenchmarkRecordingService> _logger;

    public BenchmarkRecordingService(
        IBenchmarkRecordStore store,
        ILogger<BenchmarkRecordingService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task RecordAsync(
        string artifactId,
        string uploadVariant,
        string originalFileName,
        long sizeInBytes,
        DateTimeOffset startedAtUtc,
        DateTimeOffset finishedAtUtc,
        string? personaName = null,
        Guid? benchmarkRunId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var record = new BenchmarkRecord
            {
                Id = Guid.NewGuid(),
                ArtifactId = artifactId,
                UploadVariant = uploadVariant,
                OriginalFileName = originalFileName,
                SizeInBytes = sizeInBytes,
                StartedAtUtc = startedAtUtc,
                FinishedAtUtc = finishedAtUtc,
                DurationInMilliseconds = (long)(finishedAtUtc - startedAtUtc).TotalMilliseconds,
                PersonaName = personaName,
                BenchmarkRunId = benchmarkRunId
            };

            await _store.AddAsync(record, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Benchmark-Datensatz für {FileName} (Variante {UploadVariant}) konnte nicht gespeichert werden.",
                originalFileName,
                uploadVariant);
        }
    }
}
