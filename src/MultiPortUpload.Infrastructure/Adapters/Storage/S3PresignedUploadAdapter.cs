// file: src/MultiPortUpload.Infrastructure/Storage/S3PresignedUploadAdapter.cs

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

using Amazon.S3;
using Amazon.S3.Model;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Application.Models;
using Microsoft.Extensions.Options;
using MultiPortUpload.Infrastructure.Options;
using System.IO;

public sealed class S3PresignedUploadAdapter : IUploadAdapter, IPresignedUploadPort
{
    // Interner Client (Endpoint minio:9000) – vom Server aus erreichbar, wird zum
    // Sicherstellen des Buckets genutzt. Der externe Presign-Host ist aus dem
    // Container heraus i. d. R. NICHT erreichbar, daher zwei getrennte Clients.
    private readonly IAmazonS3 _internalClient;
    private readonly IAmazonS3 _presignClient;
    private readonly S3StorageOptions _options;

    public S3PresignedUploadAdapter(
        IAmazonS3 internalClient,
        IOptions<S3StorageOptions> options)
    {
        _internalClient = internalClient;
        _options = options.Value;

        // Presigned URLs müssen für den Host signiert werden, den der EXTERNE
        // Client tatsächlich anspricht. Der interne Endpoint (z. B. minio:9000)
        // ist nur aus dem Docker-Netz erreichbar; würde man die URL client-seitig
        // auf einen anderen Host umschreiben, stimmt die SigV4-Signatur (die den
        // Host-Header einschließt) nicht mehr => 403 (SignatureDoesNotMatch).
        // Daher wird hier ein dedizierter Client verwendet, der die URL direkt
        // gegen den öffentlich erreichbaren Endpoint signiert.
        var publicEndpoint = string.IsNullOrWhiteSpace(_options.PublicEndpoint)
            ? _options.Endpoint
            : _options.PublicEndpoint;

        // Wichtig: KEIN RegionEndpoint setzen – das würde die ServiceURL überschreiben
        // und eine URL gegen s3.<region>.amazonaws.com erzeugen. Nur ServiceURL,
        // damit die presigned URL exakt gegen den öffentlichen Host signiert wird.
        var config = new AmazonS3Config
        {
            ServiceURL = publicEndpoint,
            ForcePathStyle = _options.UsePathStyle,
            UseHttp = publicEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        };

        _presignClient = new AmazonS3Client(
            _options.AccessKey,
            _options.SecretKey,
            config);
    }

    public string Variant => "S3Presigned";

    public string Description => "Erzeugt eine vorsignierte URL für den direkten Upload zum S3-Speicher.";

    public async Task<PresignedUploadResult> CreateUploadUrlAsync(
        PresignedUploadCommand command,
        CancellationToken cancellationToken = default)
    {
        var artifactId = Guid.NewGuid().ToString("N");
        var storedFileName = $"{artifactId}_{command.FileName}";
        var storagePath = $"{_options.BucketName}/{storedFileName}";

        // Der Client lädt direkt zu MinIO hoch, ohne dass der Server den Bucket
        // berührt. Daher muss der Bucket hier vorab sichergestellt werden – über
        // den internen Endpoint, den der Server erreicht.
        await EnsureBucketExistsAsync(cancellationToken);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = storedFileName,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),
            // ContentType = command.ContentType,
            Protocol = Protocol.HTTP
        };

        var uploadUrl = _presignClient.GetPreSignedURL(request);

        return new PresignedUploadResult(
            uploadUrl,
            artifactId,
            storedFileName,
            storagePath);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var response = await _internalClient.ListBucketsAsync(cancellationToken);
        var bucketList = response.Buckets ?? [];

        var exists = bucketList.Any(bucket =>
            string.Equals(bucket.BucketName, _options.BucketName, StringComparison.Ordinal));

        if (!exists)
        {
            await _internalClient.PutBucketAsync(
                new PutBucketRequest { BucketName = _options.BucketName },
                cancellationToken);
        }
    }

    public Task<UploadPortResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}