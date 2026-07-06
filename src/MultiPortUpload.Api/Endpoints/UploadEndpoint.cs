// file: src/MultiPortUpload.Api/Endpoints/UploadEndpoint.cs

using Microsoft.AspNetCore.Mvc;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Application.Abstractions;

namespace MultiPortUpload.Api.Endpoints;

public static class UploadEndpoints
{
    // Dieser Endpoint ermöglicht das Hochladen von Dateien über verschiedene Upload-Adapter, die durch den adapterName in der URL spezifiziert werden. Der Endpoint liest die Datei aus dem Formulardaten des HTTP-Requests, sucht den entsprechenden Upload-Adapter im IUploadAdapterRegistry und führt den Upload durch. Das Ergebnis des Uploads wird dann als JSON-Antwort zurückgegeben. Es ist wichtig, dass der Endpoint robust gegenüber Fehlern ist, z.B. wenn keine Datei im Request gefunden wird oder wenn der angegebene AdapterName ungültig ist, damit er entsprechende Fehlermeldungen zurückgeben kann.
    public static IEndpointRouteBuilder MapUploadEndpoints(
        this IEndpointRouteBuilder app)
    {
        // PM: 26/05/2026 - Dieser Endpoint ermöglicht das Hochladen von Dateien über verschiedene Upload-Adapter, die durch den adapterName in der URL spezifiziert werden. Der Endpoint liest die Datei aus dem Formulardaten des HTTP-Requests, sucht den entsprechenden Upload-Adapter im IUploadAdapterRegistry und führt den Upload durch. Das Ergebnis des Uploads wird dann als JSON-Antwort zurückgegeben. Es ist wichtig, dass der Endpoint robust gegenüber Fehlern ist, z.B. wenn keine Datei im Request gefunden wird oder wenn der angegebene AdapterName ungültig ist, damit er entsprechende Fehlermeldungen zurückgeben kann.
        app.MapPost("/api/uploads/{adapterName}", async (
            string adapterName,
            HttpRequest httpRequest,
            IUploadAdapterRegistry registry,
            CancellationToken cancellationToken) =>
        {
            var form = await httpRequest.ReadFormAsync(cancellationToken);

            var file = form.Files.FirstOrDefault();

            if (file is null)
            {
                return Results.BadRequest("Keine Datei im Request gefunden.");
            }

            var adapter = registry.GetByVariant(adapterName);

            await using var stream = file.OpenReadStream();

            var result = await adapter.UploadAsync(
                stream,
                file.FileName,
                cancellationToken);

            return Results.Ok(result);
        })
        .WithName("UploadWithAdapter")
        .WithTags("Uploads");

        // PM: 26/05/2026 - Dieser Endpoint ermöglicht das Hochladen von Dateien in Chunks über den ChunkedUploadAdapter. Der Endpoint liest die Datei-Chunks und die zugehörigen Metadaten (uploadId, fileName, chunkIndex, totalChunks) aus dem Formulardaten des HTTP-Requests, ruft die UploadChunkAsync-Methode des IChunkedUploadPort auf und gibt das Ergebnis als JSON-Antwort zurück. Es ist wichtig, dass der Endpoint robust gegenüber Fehlern ist, z.B. wenn keine Datei im Request gefunden wird oder wenn die erforderlichen Metadaten fehlen oder ungültig sind, damit er entsprechende Fehlermeldungen zurückgeben kann.
        app.MapPost("/api/uploads/chunked", async (
            HttpRequest request,
            IChunkedUploadPort chunkedUploadPort,
            CancellationToken cancellationToken) =>
            {
                var form = await request.ReadFormAsync(cancellationToken);

                var file = form.Files["file"];

                if (file is null)
                {
                    return Results.BadRequest("Kein Chunk empfangen.");
                }

                var uploadId = form["uploadId"].ToString();
                var fileName = form["fileName"].ToString();

                if (string.IsNullOrWhiteSpace(uploadId))
                {
                    return Results.BadRequest("uploadId fehlt.");
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Results.BadRequest("fileName fehlt.");
                }

                if (!int.TryParse(form["chunkIndex"], out var chunkIndex))
                {
                    return Results.BadRequest("chunkIndex fehlt oder ist ungültig.");
                }

                if (!int.TryParse(form["totalChunks"], out var totalChunks))
                {
                    return Results.BadRequest("totalChunks fehlt oder ist ungültig.");
                }

                await using var stream = file.OpenReadStream();

                /*
                public async Task<ChunkUploadResult> UploadChunkAsync(
                    Stream stream,
                    string uploadId,
                    string originalFileName,
                    int chunkIndex,
                    int totalChunks,
                    CancellationToken cancellationToken)
                */
                var result = await chunkedUploadPort.UploadChunkAsync(
                    stream,
                    uploadId,
                    fileName,
                    chunkIndex,
                    totalChunks,
                    cancellationToken);

                return Results.Ok(result);
            })
            .WithName("UploadWithChunkedUploadAdapter")
            .WithTags("Uploads");

        return app;
    }
}