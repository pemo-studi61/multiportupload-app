// file: src/MultiPortUpload.Api/Controllers/UploadsController.cs
using MultiPortUpload.Api.Contracts;
using MultiPortUpload.Application.Models;
using MultiPortUpload.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MultiPortUpload.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly UploadService _uploadService;
    private readonly ILogger<UploadsController> _logger;

    public UploadsController(
        UploadService uploadService,
        ILogger<UploadsController> logger)
    {
        _uploadService = uploadService;
        _logger = logger;
    }

    // Diesen Endpunkt nicht dokumentieren wg. Swagger-Fehler
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("{variant}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(1024L * 1024L * 1024L)] // 1 GB

    public async Task<ActionResult<UploadResponse>> UploadAsync(
            string variant,
            [FromForm] IFormFile file,
            [FromHeader(Name = "X-Persona-Name")] string? personaName,
            [FromHeader(Name = "X-Benchmark-Run-Uuid")] Guid? benchmarkRunUuid,
            CancellationToken cancellationToken)
        {
            if (file is null || file.Length <= 0)
            {
                return BadRequest("Es wurde keine Datei hochgeladen.");
            }

            var trustedFileName = Path.GetFileName(file.FileName);

            _logger.LogInformation(
                "Incoming upload request for {FileName} ({SizeInBytes} bytes)",
                trustedFileName,
                file.Length);

            await using var stream = file.OpenReadStream();

            var command = new UploadCommand(
                stream,
                trustedFileName,
                file.ContentType ?? "application/octet-stream",
                file.Length,
                personaName,
                benchmarkRunUuid);

            var result = await _uploadService.UploadAsync(variant, command, cancellationToken);

            var response = new UploadResponse(
                result.ArtifactId,
                result.OriginalFileName,
                result.StoredFileName,
                result.StoragePath,
                result.UploadVariant,
                result.SizeInBytes,
                result.StartedAtUtc,
                result.FinishedAtUtc,
                result.DurationInMilliseconds,
                result.Sha256);

            return Ok(response);
    }
}