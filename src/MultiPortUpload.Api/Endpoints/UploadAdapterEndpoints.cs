// file: src/MultiPortUpload.Api/Endpoints/UploadAdapterEndpoints.cs

using MultiPortUpload.Application.Ports.Outbound;

namespace MultiPortUpload.Api.Endpoints;

/// <summary>
/// Listet die registrierten Upload-Adapter (Varianten) auf.
/// </summary>
public static class UploadAdapterEndpoints
{
    public static void MapUploadAdapterEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/api/upload-adapters",
            (IUploadAdapterRegistry registry) =>
            {
                var adapters = registry.GetAll()
                    .OrderBy(a => a.Variant, StringComparer.OrdinalIgnoreCase)
                    .Select(a => new
                    {
                        name = a.Variant,
                        description = a.Description
                    })
                    .ToList();

                return Results.Ok(new
                {
                    count = adapters.Count,
                    adapters
                });
            })
            .WithName("UploadAdapters")
            .WithTags("UploadAdapters");
    }
}
