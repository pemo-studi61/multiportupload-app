// file: src/MultiPortUpload.Application/Ports/Outbound/IUploadAdapter.cs

using MultiPortUpload.Application.Abstractions;

namespace MultiPortUpload.Application.Ports.Outbound;

public interface IUploadAdapter
{
    string Variant { get; }

    /// <summary>Kurze, fachliche Beschreibung der Upload-Variante.</summary>
    string Description { get; }

    Task<UploadPortResult> UploadAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default
    );
}