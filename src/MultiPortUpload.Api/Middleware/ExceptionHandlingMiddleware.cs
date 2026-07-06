// file: src/MultiPortUpload.Api/Middleware/ExceptionHandlingMiddleware.cs

using System.Net;
using MultiPortUpload.Api.Middleware;
using MultiPortUpload.Application.Exceptions;

namespace MultiPortUpload.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;

        // Eine unbekannte Upload-Variante ist ein Client-Fehler (falscher Name),
        // kein Serverfehler – daher 400 Bad Request mit aussagekräftiger Meldung.
        if (ex is UploadVariantNotFoundException variantNotFound)
        {
            _logger.LogWarning(
                ex,
                "Unbekannte Upload-Variante '{Variant}'. TraceId={TraceId}, Path={Path}, Method={Method}",
                variantNotFound.Variant,
                traceId,
                context.Request.Path,
                context.Request.Method);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsJsonAsync(new ErrorResponse(
                Error: "UploadVariantNotFound",
                Message: variantNotFound.Message,
                TraceId: traceId,
                Timestamp: DateTime.UtcNow));

            return;
        }

        // Ein Adapter, der über den Single-Shot-Endpunkt angesprochen wird, obwohl
        // er einen mehrstufigen Ablauf erfordert (z. B. Resumable), ist ebenfalls
        // ein Client-Fehler (falscher Endpunkt) – 400 mit Hinweis statt 500.
        if (ex is UploadMethodNotSupportedException methodNotSupported)
        {
            _logger.LogWarning(
                ex,
                "Single-Shot-Upload für Variante '{Variant}' nicht unterstützt. TraceId={TraceId}, Path={Path}, Method={Method}",
                methodNotSupported.Variant,
                traceId,
                context.Request.Path,
                context.Request.Method);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsJsonAsync(new ErrorResponse(
                Error: "UploadMethodNotSupported",
                Message: methodNotSupported.Message,
                TraceId: traceId,
                Timestamp: DateTime.UtcNow));

            return;
        }

        _logger.LogError(
            ex,
            "Unhandled exception. TraceId={TraceId}, Path={Path}, Method={Method}",
            traceId,
            context.Request.Path,
            context.Request.Method);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorResponse(
            Error: "InternalServerError",
            Message: "Ein unerwarteter Fehler ist aufgetreten.",
            TraceId: traceId,
            Timestamp: DateTime.UtcNow);

        await context.Response.WriteAsJsonAsync(response);
    }
}