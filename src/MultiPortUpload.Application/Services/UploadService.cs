// file: src/MultiPortUpload.Application/Services/UploadService.cs

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Models;
using Microsoft.Extensions.Logging;

namespace MultiPortUpload.Application.Services;

public sealed class UploadService
{
    private readonly IUploadPortFactory _uploadPortFactory;
    private readonly BenchmarkRecordingService _benchmarkRecorder;
    private readonly StoredArtifactRecordingService _artifactRecorder;
    private readonly ILogger<UploadService> _logger;

    public UploadService(
        IUploadPortFactory uploadPortFactory,
        BenchmarkRecordingService benchmarkRecorder,
        StoredArtifactRecordingService artifactRecorder,
        ILogger<UploadService> logger)
    {
        _uploadPortFactory = uploadPortFactory;
        _benchmarkRecorder = benchmarkRecorder;
        _artifactRecorder = artifactRecorder;
        _logger = logger;
    }
    
    public async Task<UploadResult> UploadAsync(
        string variant,
        UploadCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var startedAtUtc = DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "Upload started for file {FileName} ({SizeInBytes} bytes) via {UploadVariant}",
            command.FileName,
            command.SizeInBytes,
            variant);

        var uploadPort = _uploadPortFactory.GetByVariant(variant);

        var portResult = await uploadPort.UploadAsync(
            command.Content,
            command.FileName,
            command.ContentType,
            cancellationToken);

        var finishedAtUtc = DateTimeOffset.UtcNow;
        var duration = finishedAtUtc - startedAtUtc;

        _logger.LogInformation(
            "Upload finished for file {FileName} in {DurationMs} ms via {UploadVariant}",
            command.FileName,
            duration.TotalMilliseconds,
            portResult.UploadVariant);

        var result = new UploadResult(
            portResult.ArtifactId,
            command.FileName,
            portResult.StoredFileName,
            portResult.StoragePath,
            portResult.UploadVariant,
            portResult.SizeInBytes,
            startedAtUtc,
            finishedAtUtc,
            (long)duration.TotalMilliseconds,
            portResult.Sha256);

        // Zuerst die Datei-Metadaten in stored_artifacts ablegen. MIME-Typ stammt aus
        // dem Upload-Kommando (Request), die Endung wird im Recorder aus dem Dateinamen
        // abgeleitet; der SHA-256-Hash ist nur bei manchen Adaptern gesetzt. Die von
        // der DB generierte Id (gen_random_uuid()) dient anschließend als artifact_id
        // des Benchmark-Datensatzes.
        var storedArtifactId = await _artifactRecorder.RecordAsync(
            result.OriginalFileName,
            result.StoredFileName,
            command.ContentType,
            result.SizeInBytes,
            result.Sha256,
            result.StoragePath,
            cancellationToken);

        var benchmarkArtifactId = storedArtifactId?.ToString() ?? result.ArtifactId;

        await _benchmarkRecorder.RecordAsync(
            benchmarkArtifactId,
            result.UploadVariant,
            result.OriginalFileName,
            result.SizeInBytes,
            result.StartedAtUtc,
            result.FinishedAtUtc,
            command.PersonaName,
            command.BenchmarkRunUuid,
            cancellationToken);

        return result;
    }
}