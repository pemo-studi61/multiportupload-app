namespace MultiPortUpload.Infrastructure.Options;

public sealed class LocalStorageOptions
{
    public const string SectionName = "LocalStorage";

    public string RootPath { get; set; } = "uploads";
}