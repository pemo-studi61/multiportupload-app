// file: src/MultiPortUpload.Application/Models/BenchmarkRunDetails.cs

namespace MultiPortUpload.Application.Models;

/// <summary>
/// Detailansicht eines Benchmark-Laufs inkl. der ihm zugeordneten Uploads.
/// Read-Modell, das vom Endpoint <c>GET /api/benchmark-runs/{id}</c> zurückgegeben
/// wird. Aggregiert die Lauf-Metadaten (<see cref="Domain.Entities.BenchmarkRun"/>)
/// mit den zugehörigen Upload-Datensätzen (<see cref="Domain.Entities.BenchmarkRecord"/>),
/// die über die RunUuid verknüpft sind.
/// </summary>
public sealed record BenchmarkRunDetails(
    long Id,
    DateTimeOffset CreatedAt,
    string? Description,
    string? RunnerName,
    float? RunningTime,
    string? Location,
    string? Comment,
    Guid RunUuid,
    int UploadCount,
    IReadOnlyList<BenchmarkRunUpload> Uploads);

/// <summary>
/// Einzelner, einem Benchmark-Lauf zugeordneter Upload innerhalb von
/// <see cref="BenchmarkRunDetails"/>.
/// </summary>
public sealed record BenchmarkRunUpload(
    string ArtifactId,
    string OriginalFileName,
    string UploadVariant,
    string? PersonaName,
    long SizeInBytes,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset FinishedAtUtc,
    long DurationInMilliseconds,
    double ThroughputMbPerSecond);
