// file: src/MultiPortUpload.Application/Exceptions/UploadVariantNotFoundException.cs

namespace MultiPortUpload.Application.Exceptions;

public sealed class UploadVariantNotFoundException : Exception
{
    public UploadVariantNotFoundException(string variant)
        : base(
            $"Keine Upload-Variante mit dem Namen '{variant}' gefunden.")
    {
        Variant = variant;
    }

    public string Variant { get; }
}