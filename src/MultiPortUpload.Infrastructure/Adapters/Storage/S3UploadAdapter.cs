// file: src/MultiportUpload.Infrastructure/Storage/S3UploadAdapter.cs

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiPortUpload.Application.Abstractions;
using MultiPortUpload.Application.Ports.Outbound;
using MultiPortUpload.Application.Logging;
using MultiPortUpload.Infrastructure.Options;
using System.Security.Cryptography;

namespace MultiPortUpload.Infrastructure.Adapters.Storage;

public sealed class S3UploadAdapter : IUploadAdapter, IUploadPort
{
    private readonly S3StorageOptions _options;
    private readonly ILogger<S3UploadAdapter> _logger;

    public string Variant => "S3";

    public string Description => "Lädt die Datei in einen S3-kompatiblen Objektspeicher (z. B. MinIO) hoch.";

    public S3UploadAdapter(
        IOptions<S3StorageOptions> options,
        ILogger<S3UploadAdapter> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

    public async Task<UploadPortResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var artifactId = Guid.NewGuid().ToString();
        var storedFileName = $"{artifactId}_{fileName}";

        var config = new AmazonS3Config
        {
            ServiceURL = _options.Endpoint,
            ForcePathStyle = _options.UsePathStyle,
            AuthenticationRegion = "us-east-1"
        };

        using var client = new AmazonS3Client(
            _options.AccessKey,
            _options.SecretKey,
            config);

        await EnsureBucketExistsAsync(client, cancellationToken);

        // 09/06/2024: Wichtig: Der SHA256-Hash muss vor dem Upload berechnet werden, da der S3-Client den Stream während des Uploads liest.
        var sha256Hash = await ComputeSha256Async(stream, cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storedFileName,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = false
        };

        // 09/06/2024: Wichtig: Der S3-Client liest den Stream erst, wenn PutObjectAsync aufgerufen wird.
        if (stream.CanSeek)
        {
            request.Headers.ContentLength = stream.Length;
        }

        try
        {
            await client.PutObjectAsync(request, cancellationToken);

            _logger.LogInformation(
                "S3 upload finished: {StoredFileName} to bucket {BucketName}",
                storedFileName,
                _options.BucketName);

        } catch (SystemException ex) {
            _logger.LogError(
                UploadEventIds.S3UploadFailed,
                ex,
                "S3UploadAdapter->UploadAsync: Allgemeiner Fehler"
            );
        }

        return new UploadPortResult(
            artifactId,
            storedFileName,
            $"s3://{_options.BucketName}/{storedFileName}",
            stream.CanSeek ? stream.Length : 0,
            Variant,
            sha256Hash);
    }

    private static async Task<string> ComputeSha256Async(
            Stream stream,
            CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var sha256 = SHA256.Create();

        var hashBytes =
            await sha256.ComputeHashAsync(stream, cancellationToken);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return Convert.ToHexString(hashBytes);
    }

    private async Task EnsureBucketExistsAsync(
     IAmazonS3 client,
     CancellationToken cancellationToken)
    {
        var response = await client.ListBucketsAsync(cancellationToken);

        var bucketList = response.Buckets ?? [];

        var exists = bucketList.Any(bucket =>
            string.Equals(
                bucket.BucketName,
                _options.BucketName,
                StringComparison.Ordinal));

        if (!exists)
        {
            await client.PutBucketAsync(
                new PutBucketRequest
                {
                    BucketName = _options.BucketName
                },
                cancellationToken);
        }
    }

    public Task<UploadPortResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}