// file: src/MultiPortUpload.Api/Middleware/ErrorResponse.cs

namespace MultiPortUpload.Api.Middleware;

public sealed record ErrorResponse(
    string Error,
    string Message,
    string TraceId,
    DateTime Timestamp);