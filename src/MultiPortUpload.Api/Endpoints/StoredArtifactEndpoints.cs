// file: src/MultiPortUpload.Api/Endpoints/StoredArtifactEndpoints.cs

using MultiPortUpload.Application.Ports.Outbound;

namespace MultiPortUpload.Api.Endpoints;

/// <summary>
/// Endpunkte zum Abfragen der persistierten Datei-Metadaten (stored_artifacts).
/// Das Anlegen erfolgt automatisch beim Upload (siehe UploadEndpoints).
/// </summary>
public static class StoredArtifactEndpoints
{
    public static void MapStoredArtifactEndpoints(this WebApplication app)
    {
        app.MapGet("/api/stored-artifacts",
            async (
                IStoredArtifactStore store,
                CancellationToken cancellationToken,
                int? limit) =>
            {
                var artifacts = await store.GetRecentAsync(limit ?? 100, cancellationToken);
                return Results.Ok(artifacts);
            })
            .WithName("GetStoredArtifacts")
            .WithTags("StoredArtifacts");

        app.MapGet("/api/stored-artifacts/{id:guid}",
            async (
                Guid id,
                IStoredArtifactStore store,
                CancellationToken cancellationToken) =>
            {
                var artifact = await store.GetByIdAsync(id, cancellationToken);
                return artifact is null ? Results.NotFound() : Results.Ok(artifact);
            })
            .WithName("GetStoredArtifactById")
            .WithTags("StoredArtifacts");
    }
}
