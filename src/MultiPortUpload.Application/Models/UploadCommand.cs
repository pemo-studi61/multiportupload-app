// file: src/MultiPortUpload.Application/Models/UploadCommand.cs

namespace MultiPortUpload.Application.Models;

public sealed record UploadCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeInBytes,
    string? PersonaName = null,
    Guid? BenchmarkRunUuid = null);