// file: src/MultiPortUpload.Infrastructure/Adapters/Storage/ResumableUploadAdapter.cs

using System.Security.Cryptography;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Exceptions;
using MultiPortUpload.Application.Models;
using MultiPortUpload.Application.Logging;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Infrastructure.Options;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class ResumableUploadAdapter : IUploadAdapter, IUploadPort
{
    public string Variant => "Resumable";

    public string Description => "Unterstützt fortsetzbare Uploads, die nach einer Unterbrechung wieder aufgenommen werden können.";

    // IUploadPort-Variante (mit contentType).
    public Task<UploadPortResult> UploadAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        throw new UploadMethodNotSupportedException(
            Variant,
            "/api/uploads/resumable/start");
    }

    // IUploadAdapter-Variante (ohne contentType) – wird nur benötigt, damit der
    // Adapter in der UploadAdapterRegistry auftaucht und über /api/upload-adapters
    // gelistet wird. Der eigentliche Upload läuft über die Resumable-Endpunkte.
    public Task<UploadPortResult> UploadAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        throw new UploadMethodNotSupportedException(
            Variant,
            "/api/uploads/resumable/start");
    }
}