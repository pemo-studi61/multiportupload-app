// file: src/MultiPortUpload.Api/Controllers/ResumableUploadsController.cs

using Microsoft.AspNetCore.Mvc;
using MultiPortUpload.Api.Contracts.Resumable;
using MultiPortUpload.Application.Services;

[ApiController]
[Route("api/uploads/resumable")]
public sealed class ResumableUploadsController : ControllerBase
{
    // Entspricht ResumableUploadAdapter.Variant; wird im Benchmark-Datensatz als
    // upload_variant gespeichert, damit Resumable-Uploads konsistent zu den
    // übrigen Adaptern (Chunked, Presigned) ausgewertet werden können.
    private const string UploadVariant = "Resumable";

    private readonly ResumableUploadSessionService _sessionService;
    private readonly BenchmarkRecordingService _benchmarkRecorder;

    public ResumableUploadsController(
        ResumableUploadSessionService sessionService,
        BenchmarkRecordingService benchmarkRecorder)
    {
        _sessionService = sessionService;
        _benchmarkRecorder = benchmarkRecorder;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartResumableUploadResponse>> StartAsync(
        [FromBody] StartResumableUploadRequest request,
        CancellationToken cancellationToken)
    {
        var session = await _sessionService.StartSessionAsync(
            request.FileName,
            request.SizeInBytes,
            cancellationToken);

        return Ok(new StartResumableUploadResponse
        {
            UploadId = session.UploadId,
            ChunkSizeBytes = session.ChunkSizeBytes,
            TotalChunks = session.TotalChunks
        });
    }

    [HttpPost("{uploadId}/chunks/{chunkIndex:int}")]
    public async Task<IActionResult> UploadChunkAsync(
        string uploadId,
        int chunkIndex,
        CancellationToken cancellationToken)
    {
        await _sessionService.SaveChunkAsync(
            uploadId,
            chunkIndex,
            Request.Body,
            cancellationToken);

        return Ok(new
        {
            uploadId,
            chunkIndex,
            success = true
        });
    }

    [HttpGet("{uploadId}/status")]
public async Task<ActionResult<ResumableUploadStatusResponse>> GetStatusAsync(
    string uploadId,
    CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetSessionAsync(
            uploadId,
            cancellationToken);

        var missingChunks = await _sessionService.GetMissingChunksAsync(
            uploadId,
            cancellationToken);

        return Ok(new ResumableUploadStatusResponse
        {
            UploadId = session.UploadId,
            TotalChunks = session.TotalChunks,
            ReceivedChunks = session.ReceivedChunks,
            MissingChunks = missingChunks
        });
    }

    [HttpPost("{uploadId}/complete")]
    public async Task<IActionResult> CompleteAsync(
        string uploadId,
        [FromHeader(Name = "X-Persona-Name")] string? personaName,
        [FromHeader(Name = "X-Benchmark-Run-Uuid")] Guid? benchmarkRunUuid,
        CancellationToken cancellationToken)
    {
        // Session-Metadaten (Dateiname, Größe, Startzeitpunkt) vor dem
        // Zusammenführen lesen – sie werden für den Benchmark-Datensatz benötigt.
        var session = await _sessionService.GetSessionAsync(
            uploadId,
            cancellationToken);

        var completedFilePath = await _sessionService.CompleteAsync(
            uploadId,
            cancellationToken);

        var finishedAtUtc = DateTimeOffset.UtcNow;

        // Anders als beim Chunked-Upload laufen die Chunks hier über den Server.
        // Die Dauer misst daher die gesamte serverseitig beobachtete Übertragung
        // (Session-Start bis Abschluss), nicht nur die Finalisierung.
        var startedAtUtc = new DateTimeOffset(
            DateTime.SpecifyKind(session.StartedAtUtc, DateTimeKind.Utc));

        // Benchmark-Datensatz erfassen – analog zu Chunked- und Presigned-Upload,
        // damit der Resumable-Adapter in den Benchmark-Auswertungen erscheint. Die
        // UploadId dient als ArtifactId.
        await _benchmarkRecorder.RecordAsync(
            uploadId,
            UploadVariant,
            session.OriginalFileName,
            session.SizeInBytes,
            startedAtUtc,
            finishedAtUtc,
            personaName,
            benchmarkRunUuid,
            cancellationToken);

        return Ok(new
        {
            uploadId,
            completedFilePath,
            success = true
        });
    }
}