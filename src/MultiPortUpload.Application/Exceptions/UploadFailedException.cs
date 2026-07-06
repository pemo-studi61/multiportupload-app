// file: src/MultiPortUpload.Application/Exceptions/UploadFailedException.cs

namespace MultiPortUpload.Application.Exceptions;

public sealed class UploadFailedException : Exception
{
    public UploadFailedException(string message)
        : base(message)
    {
    }

    public UploadFailedException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}