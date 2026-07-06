// file: src/MultiPortUpload.Application/Ports/Outbound/IUploadAdapterRegistry.cs

namespace MultiPortUpload.Application.Ports.Outbound;

public interface IUploadAdapterRegistry
{
    IUploadAdapter GetByVariant(string name);

    IReadOnlyCollection<IUploadAdapter> GetAll();
}