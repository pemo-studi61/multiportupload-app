// file: /src/MultiportUpload.Infrastructure/Options/S3StorageOptions.cs

namespace MultiPortUpload.Infrastructure.Options;

public sealed class S3StorageOptions
{
    // Interner Endpoint, den der Server selbst nutzt, um MinIO/S3 zu erreichen
    // (im Docker-Netz z. B. http://minio:9000).
    public string Endpoint { get; set; } = "";

    // Öffentlich erreichbarer Endpoint, unter dem ein EXTERNER Client S3/MinIO
    // erreicht (z. B. http://<host>:9000). Presigned URLs werden gegen diesen
    // Host signiert, damit die SigV4-Signatur (die den Host-Header einschließt)
    // beim direkten Client-Upload gültig ist. Leer => Fallback auf Endpoint.
    public string PublicEndpoint { get; set; } = "";

    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "multiportupload";
    public bool UsePathStyle { get; set; } = true;
}