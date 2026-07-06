// file: src/MultiPortUpload.Application/Models/PresignedUploadCompletionCommand.cs

namespace MultiPortUpload.Application.Models;

/// <summary>
/// Vom Client gesendete Abschlussmeldung für einen Presigned-S3-Upload. Da der
/// eigentliche PUT direkt an S3 erfolgt und der Server davon nichts erfährt,
/// meldet der Client hier die gemessene Transferdauer (Start/Ende) sowie die
/// Dateigröße zurück, damit ein Benchmark-Datensatz erfasst werden kann.
/// </summary>
public sealed record PresignedUploadCompletionCommand(
    string ArtifactId,
    string OriginalFileName,
    long SizeInBytes,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset FinishedAtUtc);
