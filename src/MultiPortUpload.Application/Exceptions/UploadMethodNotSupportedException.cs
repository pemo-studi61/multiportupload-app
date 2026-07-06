// file: src/MultiPortUpload.Application/Exceptions/UploadMethodNotSupportedException.cs

namespace MultiPortUpload.Application.Exceptions;

/// <summary>
/// Wird geworfen, wenn ein Adapter über den Single-Shot-Endpunkt
/// <c>/api/uploads/{variant}</c> angesprochen wird, obwohl er einen mehrstufigen
/// Ablauf über eigene Endpunkte erfordert (z. B. der Resumable-Upload). Das ist
/// ein Client-Fehler (falscher Endpunkt) und wird daher als 400 Bad Request mit
/// einem Hinweis auf den korrekten Endpunkt beantwortet – nicht als 500.
/// </summary>
public sealed class UploadMethodNotSupportedException : Exception
{
    public UploadMethodNotSupportedException(string variant, string correctEndpoint)
        : base(
            $"Die Upload-Variante '{variant}' unterstützt keinen Single-Shot-Upload über " +
            $"/api/uploads/{variant}. Bitte den mehrstufigen Ablauf über '{correctEndpoint}' verwenden.")
    {
        Variant = variant;
        CorrectEndpoint = correctEndpoint;
    }

    public string Variant { get; }

    public string CorrectEndpoint { get; }
}
