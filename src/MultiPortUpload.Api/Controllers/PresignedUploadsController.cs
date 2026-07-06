// file: src/MultiPortUpload.Api/Contracts/PresignedUploadsController.cs

namespace MultiPortUpload.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using MultiPortUpload.Api.Contracts;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Models;
using MultiPortUpload.Application.Services;

[ApiController]
[Route("api/uploads/presigned-url")]
public sealed class PresignedUploadsController : ControllerBase
{
    // Entspricht S3PresignedUploadAdapter.Variant; wird im Benchmark-Datensatz
    // als upload_variant gespeichert, damit Presigned-Uploads konsistent zu den
    // übrigen Adaptern ausgewertet werden können.
    private const string UploadVariant = "S3Presigned";

    private readonly IPresignedUploadPort _presignedUploadPort;
    private readonly BenchmarkRecordingService _benchmarkRecorder;

    public PresignedUploadsController(
        IPresignedUploadPort presignedUploadPort,
        BenchmarkRecordingService benchmarkRecorder)
    {
        _presignedUploadPort = presignedUploadPort;
        _benchmarkRecorder = benchmarkRecorder;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<PresignedUploadResponse>> CreateUploadUrl(
        [FromBody] PresignedUploadCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _presignedUploadPort.CreateUploadUrlAsync(
            command,
            cancellationToken);

        var response = new PresignedUploadResponse(
            result.UploadUrl,
            result.ArtifactId,
            result.StoredFileName,
            result.StoragePath);

        return Ok(response);
    }

    // Der PUT erfolgt direkt an S3, der Server erfährt davon nichts. Der Client
    // meldet daher den Abschluss inkl. der von ihm gemessenen Transferdauer, damit
    // ein Benchmark-Datensatz – analog zu den anderen Adaptern – erfasst wird.
    [HttpPost("complete")]
    [Consumes("application/json")]
    public async Task<IActionResult> CompleteUpload(
        [FromBody] PresignedUploadCompletionCommand command,
        [FromHeader(Name = "X-Persona-Name")] string? personaName,
        [FromHeader(Name = "X-Benchmark-Run-Uuid")] Guid? benchmarkRunUuid,
        CancellationToken cancellationToken)
    {
        await _benchmarkRecorder.RecordAsync(
            command.ArtifactId,
            UploadVariant,
            command.OriginalFileName,
            command.SizeInBytes,
            command.StartedAtUtc,
            command.FinishedAtUtc,
            personaName,
            benchmarkRunUuid,
            cancellationToken);

        return NoContent();
    }
}