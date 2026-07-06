// file: src/MultiPortUpload.Application/Models/UploadQueueItem.cs

namespace MultiPortUpload.Application.Models;

public record UploadQueueItem(
    string UploadId,
    string TempFilePath,
    string OriginalFileName,
    string UploadVariant,
    DateTime EnqueuedAtUtc
);