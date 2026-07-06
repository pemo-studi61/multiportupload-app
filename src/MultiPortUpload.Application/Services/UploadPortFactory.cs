// file: src/MultiPortUpload.Application/Services/UploadPortFactory.cs

using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Exceptions;

using Microsoft.Extensions.Logging;

namespace MultiPortUpload.Application.Services;

public sealed class UploadPortFactory : IUploadPortFactory
{
    // Toleriert gebräuchliche Kurznamen (z. B. aus der Benchmark-Konfiguration),
    // die nicht exakt der Variant-Bezeichnung des Adapters entsprechen.
    private static readonly IReadOnlyDictionary<string, string> VariantAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["local"] = "LocalFile",
            ["stream"] = "Streaming",
            ["presigned-s3"] = "S3Presigned",
            ["s3"] = "S3"
        };

    private readonly IEnumerable<IUploadPort> _uploadPorts;

    public UploadPortFactory(IEnumerable<IUploadPort> uploadPorts)
    {
        _uploadPorts = uploadPorts;
    }

    public IUploadPort GetByVariant(string variant)
    {
        var resolvedVariant = VariantAliases.TryGetValue(variant, out var canonical)
            ? canonical
            : variant;

        var uploadPort = _uploadPorts.FirstOrDefault(port =>
            string.Equals(port.Variant, resolvedVariant, StringComparison.OrdinalIgnoreCase));

        if (uploadPort is null)
        {
            // Eigene Exception, damit wir im Controller einen spezifischen Fehlerstatuscode zurückgeben können, wenn die Upload-Variante nicht gefunden wird, anstatt eines generischen 500er-Fehlers. Es ist wichtig, dass die UploadPortFactory eine spezifische Ausnahme wirft, damit der Controller diese Ausnahme abfangen und entsprechend darauf reagieren kann, z.B. indem er einen 400 Bad Request zurückgibt, wenn die angeforderte Upload-Variante ungültig ist. Wenn wir stattdessen eine generische Ausnahme wie InvalidOperationException werfen würden, wäre es schwieriger für den Controller zu erkennen, dass es sich um ein Problem mit der Upload-Variante handelt, und er könnte fälschlicherweise einen 500 Internal Server Error zurückgeben, was nicht die richtige Antwort für diesen Fall wäre.
            throw new UploadVariantNotFoundException(variant);
        }

        return uploadPort;
    }
}