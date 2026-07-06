// file: src/MultiPortUpload.Api/Controllers/ChunkedUploadsController.cs

using Microsoft.AspNetCore.Mvc;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Services;

namespace MultiPortUpload.Api.Controllers;

[ApiController]
[Route("api/uploads/chunked")]
public sealed class ChunkedUploadsController : ControllerBase
{
    private readonly IChunkedUploadPort _chunkedUploadPort;
    private readonly BenchmarkRecordingService _benchmarkRecorder;

    public ChunkedUploadsController(
        IChunkedUploadPort chunkedUploadPort,
        BenchmarkRecordingService benchmarkRecorder)
    {
        _chunkedUploadPort = chunkedUploadPort;
        _benchmarkRecorder = benchmarkRecorder;
    }

    [HttpPost("{uploadId}/chunk")]
    public async Task<ActionResult<ChunkUploadResult>> UploadChunkAsync(
        string uploadId,
        IFormFile file,
        [FromForm] string originalFileName,
        [FromForm] int chunkIndex,
        [FromForm] int totalChunks,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("Leerer Chunk empfangen.");
        }

        await using var stream = file.OpenReadStream();

        var result = await _chunkedUploadPort.UploadChunkAsync(
            stream,
            uploadId,
            originalFileName,
            chunkIndex,
            totalChunks,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{uploadId}/complete")]
    public async Task<ActionResult<UploadPortResult>> CompleteAsync(
        string uploadId,
        [FromForm] string originalFileName,
        [FromForm] int totalChunks,
        [FromHeader(Name = "X-Persona-Name")] string? personaName,
        [FromHeader(Name = "X-Benchmark-Run-Uuid")] Guid? benchmarkRunUuid,
        CancellationToken cancellationToken)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;

        var result = await _chunkedUploadPort.CompleteAsync(
            uploadId,
            originalFileName,
            totalChunks,
            cancellationToken);

        var finishedAtUtc = DateTimeOffset.UtcNow;

        // Benchmark-Datensatz für den Chunked-Upload erfassen. Hinweis: Die Dauer
        // misst die serverseitige Finalisierung (Zusammenführen der Chunks), nicht
        // die gesamte Übertragungszeit – diese kennt nur der Client.
        await _benchmarkRecorder.RecordAsync(
            result.ArtifactId,
            result.UploadVariant,
            originalFileName,
            result.SizeInBytes,
            startedAtUtc,
            finishedAtUtc,
            personaName,
            benchmarkRunUuid,
            cancellationToken);

        return Ok(result);
    }
}