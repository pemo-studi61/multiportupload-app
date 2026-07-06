// file: src/MultiPortUpload.Application/Abstractions/IUploadPortFactory.cs

namespace MultiPortUpload.Application.Abstractions;

public interface IUploadPortFactory
{
    IUploadPort GetByVariant(string variant);
}