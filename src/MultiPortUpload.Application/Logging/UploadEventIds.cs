// file: src/MultiPortUpload.Application/Logging/UploadEventIds.cs

using System.Net;
using Microsoft.Extensions.Logging;

namespace MultiPortUpload.Application.Logging;

public static class UploadEventIds
{
    public static readonly EventId AdapterSelected =
        new(1000, nameof(AdapterSelected));

    public static readonly EventId UploadStarted =
        new(1100, nameof(UploadStarted));

    public static readonly EventId UploadCompleted =
        new(1101, nameof(UploadCompleted));

    public static readonly EventId UploadFailed =
        new(1199, nameof(UploadFailed));

    public static readonly EventId UploadCleanupSuccess =
        new(1200, nameof(UploadCleanupSuccess));

    public static readonly EventId UploadCleanupFailed =
        new(1201, nameof(UploadCleanupFailed));
    
    public static readonly EventId UploadConflictDetected =
        new(1202, nameof(UploadConflictDetected));

    public static readonly EventId UploadConflictResolved =
        new(1203, nameof(UploadConflictResolved));

    public static readonly EventId UploadConflictCheckFailed =
        new(1204, nameof(UploadConflictCheckFailed));

    public static readonly EventId MemoryUploadFailed = 
       new (1206, nameof(MemoryUploadFailed));

    public static readonly EventId QueueBasedUploadFailed =
       new (1208, nameof(QueueBasedUploadFailed));

    public static readonly EventId S3UploadFailed =
     new (1210, nameof(S3UploadFailed));

    public static readonly EventId S3UploadCompleted =
     new (1212, nameof(S3UploadCompleted));

    public static readonly EventId S3PresignedUploadFailed =
     new (1214, nameof(S3PresignedUploadFailed));

    public static readonly EventId S3PresignedUploadCompleted =
     new (1216, nameof(S3PresignedUploadCompleted));

    public static readonly EventId StreamingUploadFailed =
     new (1218, nameof(StreamingUploadFailed));

    public static readonly EventId StreamingUploadCompleted =
     new (1220, nameof(StreamingUploadCompleted));

    public static readonly EventId ViruscanUploadFailed =
     new (1222, nameof(ViruscanUploadFailed));

    public static readonly EventId ViruscanUploadCompleted =
     new (1224, nameof(ViruscanUploadCompleted));

    public static readonly EventId ChunkedUploadCompleted =
     new (1226, nameof(ChunkedUploadCompleted));

    public static readonly EventId ChunkedUploadFailed =
     new (1228, nameof(ChunkedUploadFailed));

    public static readonly EventId BenchmarkStarted =
        new(2000, nameof(BenchmarkStarted));

    public static readonly EventId BenchmarkCompleted =
        new(2001, nameof(BenchmarkCompleted));
}