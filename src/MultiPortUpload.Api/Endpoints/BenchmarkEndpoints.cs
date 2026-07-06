// file: src/MultiPortUpload.Api/Endpoints/BenchmarkEndpoints.cs

using MultiPortUpload.Application.Models;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Domain.Entities;

namespace MultiPortUpload.Api.Endpoints;

/// <summary>
/// Request-Body zum Anlegen eines Benchmark-Laufs. Alle Felder sind optional;
/// Id, CreatedAt und RunUuid werden von der Datenbank generiert.
/// </summary>
public sealed record CreateBenchmarkRunRequest(
    string? Description,
    string? RunnerName,
    float? RunningTime,
    string? Location,
    string? Comment);

/// <summary>
/// Endpunkte zum Abfragen der persistierten Benchmark-Datensätze.
/// </summary>
public static class BenchmarkEndpoints
{
    public static void MapBenchmarkEndpoints(this WebApplication app)
    {
        app.MapGet("/api/benchmarks",
            async (
                IBenchmarkRecordStore store,
                CancellationToken cancellationToken,
                int? limit,
                string? variant) =>
                {
                    var records = await store.GetRecentAsync(
                        limit ?? 100,
                        variant,
                        cancellationToken);

                    var response = records.Select(r => new
                    {
                        r.Id,
                        r.ArtifactId,
                        r.UploadVariant,
                        r.OriginalFileName,
                        r.SizeInBytes,
                        r.StartedAtUtc,
                        r.FinishedAtUtc,
                        r.DurationInMilliseconds,
                        ThroughputMbPerSecond = Math.Round(r.ThroughputMbPerSecond, 3)
                    });

                return Results.Ok(response);
            })
            .WithName("GetBenchmarks")
            .WithTags("Benchmarks");

        app.MapGet("/api/benchmarks/summary",
            async (IBenchmarkRecordStore store, CancellationToken cancellationToken) =>
            {
                var summary = await store.GetSummaryAsync(cancellationToken);

                var response = summary.Select(s => new
                {
                    s.UploadVariant,
                    s.UploadCount,
                    AverageDurationMs = Math.Round(s.AverageDurationMs, 2),
                    s.MinDurationMs,
                    s.MaxDurationMs,
                    P95DurationMs = Math.Round(s.P95DurationMs, 2),
                    s.TotalBytes
                });

                return Results.Ok(response);
            })
            .WithName("GetBenchmarkSummary")
            .WithTags("Benchmarks");

        app.MapGet("/api/benchmark-runs",
            async (
                IBenchmarkRunStore store,
                CancellationToken cancellationToken,
                int? limit) =>
            {
                var runs = await store.GetRecentAsync(limit ?? 100, cancellationToken);

                var response = runs.Select(r => new
                {
                    r.Id,
                    r.CreatedAt,
                    r.Description,
                    r.RunnerName,
                    r.RunningTime,
                    r.Location,
                    r.Comment,
                    r.RunUuid
                });

                return Results.Ok(response);
            })
            .WithName("GetBenchmarkRuns")
            .WithTags("Benchmarks");

        app.MapGet("/api/benchmark-runs/{id:long}",
            async (
                long id,
                IBenchmarkRunStore runStore,
                IBenchmarkRecordStore recordStore,
                CancellationToken cancellationToken) =>
            {
                var run = await runStore.GetByIdAsync(id, cancellationToken);

                if (run is null)
                {
                    return Results.NotFound();
                }

                var records = await recordStore.GetByRunUuidAsync(run.RunUuid, cancellationToken);

                var uploads = records
                    .Select(r => new BenchmarkRunUpload(
                        r.ArtifactId,
                        r.OriginalFileName,
                        r.UploadVariant,
                        r.PersonaName,
                        r.SizeInBytes,
                        r.StartedAtUtc,
                        r.FinishedAtUtc,
                        r.DurationInMilliseconds,
                        Math.Round(r.ThroughputMbPerSecond, 3)))
                    .ToList();

                var response = new BenchmarkRunDetails(
                    run.Id,
                    run.CreatedAt,
                    run.Description,
                    run.RunnerName,
                    run.RunningTime,
                    run.Location,
                    run.Comment,
                    run.RunUuid,
                    uploads.Count,
                    uploads);

                return Results.Ok(response);
            })
            .WithName("GetBenchmarkRunById")
            .WithTags("Benchmarks");

        app.MapPost("/api/benchmark-runs",
            async (
                CreateBenchmarkRunRequest request,
                IBenchmarkRunStore store,
                CancellationToken cancellationToken) =>
            {
                var run = new BenchmarkRun
                {
                    Description = request.Description,
                    RunnerName = request.RunnerName,
                    RunningTime = request.RunningTime,
                    Location = request.Location,
                    Comment = request.Comment
                };

                var created = await store.CreateAsync(run, cancellationToken);

                var response = new
                {
                    created.Id,
                    created.CreatedAt,
                    created.Description,
                    created.RunnerName,
                    created.RunningTime,
                    created.Location,
                    created.Comment,
                    created.RunUuid
                };

                return Results.Created($"/api/benchmark-runs/{created.Id}", response);
            })
            .WithName("CreateBenchmarkRun")
            .WithTags("Benchmarks");

        app.MapDelete("/api/benchmark-runs/{id:long}",
            async (
                long id,
                IBenchmarkRunStore store,
                CancellationToken cancellationToken) =>
            {
                var deleted = await store.DeleteAsync(id, cancellationToken);

                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteBenchmarkRun")
            .WithTags("Benchmarks");
    }
}
