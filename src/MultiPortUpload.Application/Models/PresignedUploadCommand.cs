// file: /src/MultiPortUpload.Application/Models/PresignedUploadCommand.cs

namespace MultiPortUpload.Application.Models;

public sealed record PresignedUploadCommand(
    string FileName,
    string ContentType);