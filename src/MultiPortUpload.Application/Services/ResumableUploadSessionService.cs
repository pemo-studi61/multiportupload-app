// file: src/MultiPortUpload.Application/Services/ResumableUploadSessionService.cs

using System.Text.Json;
using MultiPortUpload.Application.Models;

namespace MultiPortUpload.Application.Services;

public sealed class ResumableUploadSessionService
{
    private const int DefaultChunkSizeBytes = 1024 * 1024;

    private readonly string _basePath;

    public ResumableUploadSessionService()
    {
        _basePath = InitializeBasePath();
    }

    private static string InitializeBasePath()
    {
        var candidates = new[]
        {
            Path.Combine("storage", "resumable"),
            Path.Combine(Path.GetTempPath(), "MultiPortUpload", "resumable")
        };

        foreach (var candidate in candidates)
        {
            try
            {
                Directory.CreateDirectory(candidate);
                return candidate;
            }
            catch
            {
                // Nächsten Kandidaten versuchen.
            }
        }

        throw new IOException("Could not create a writable resumable storage directory.");
    }

    public async Task<ResumableUploadSession> StartSessionAsync(
        string originalFileName,
        long sizeInBytes,
        CancellationToken cancellationToken)
    {
        var totalChunks = (int)Math.Ceiling(sizeInBytes / (double)DefaultChunkSizeBytes);

        var session = new ResumableUploadSession
        {
            OriginalFileName = originalFileName,
            SizeInBytes = sizeInBytes,
            ChunkSizeBytes = DefaultChunkSizeBytes,
            TotalChunks = totalChunks
        };

        var sessionPath = GetSessionPath(session.UploadId);
        var chunksPath = GetChunksPath(session.UploadId);

        Directory.CreateDirectory(sessionPath);
        Directory.CreateDirectory(chunksPath);

        await SaveSessionAsync(session, cancellationToken);

        return session;
    }

    public async Task<ResumableUploadSession> GetSessionAsync(
        string uploadId,
        CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(uploadId);

        if (!File.Exists(metadataPath))
        {
            throw new InvalidOperationException($"Upload-Session nicht gefunden: {uploadId}");
        }

        await using var stream = File.OpenRead(metadataPath);

        var session = await JsonSerializer.DeserializeAsync<ResumableUploadSession>(
            stream,
            cancellationToken: cancellationToken);

        return session ?? throw new InvalidOperationException(
            $"Upload-Session konnte nicht gelesen werden: {uploadId}");
    }

    public async Task SaveChunkAsync(
        string uploadId,
        int chunkIndex,
        Stream chunkStream,
        CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(uploadId, cancellationToken);

        if (chunkIndex < 0 || chunkIndex >= session.TotalChunks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunkIndex),
                $"Ungültiger ChunkIndex {chunkIndex} für Upload {uploadId}.");
        }

        var chunkPath = GetChunkPath(uploadId, chunkIndex);

        await using (var fileStream = File.Create(chunkPath))
        {
            await chunkStream.CopyToAsync(fileStream, cancellationToken);
        }

        if (!session.ReceivedChunks.Contains(chunkIndex))
        {
            session.ReceivedChunks.Add(chunkIndex);
            session.ReceivedChunks.Sort();
            await SaveSessionAsync(session, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<int>> GetMissingChunksAsync(
        string uploadId,
        CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(uploadId, cancellationToken);

        var received = session.ReceivedChunks.ToHashSet();

        return Enumerable.Range(0, session.TotalChunks)
            .Where(index => !received.Contains(index))
            .ToList();
    }

    public async Task<string> CompleteAsync(
        string uploadId,
        CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(uploadId, cancellationToken);

        var missingChunks = await GetMissingChunksAsync(uploadId, cancellationToken);

        if (missingChunks.Count > 0)
        {
            throw new InvalidOperationException(
                $"Upload ist unvollständig. Fehlende Chunks: {string.Join(", ", missingChunks)}");
        }

        var completedPath = Path.Combine(
            GetSessionPath(uploadId),
            session.OriginalFileName);

        await using var outputStream = File.Create(completedPath);

        for (var index = 0; index < session.TotalChunks; index++)
        {
            var chunkPath = GetChunkPath(uploadId, index);

            await using var inputStream = File.OpenRead(chunkPath);
            await inputStream.CopyToAsync(outputStream, cancellationToken);
        }

        return completedPath;
    }

    private async Task SaveSessionAsync(
        ResumableUploadSession session,
        CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(session.UploadId);

        await using var stream = File.Create(metadataPath);

        await JsonSerializer.SerializeAsync(
            stream,
            session,
            new JsonSerializerOptions { WriteIndented = true },
            cancellationToken);
    }

    private string GetSessionPath(string uploadId)
        => Path.Combine(_basePath, uploadId);

    private string GetChunksPath(string uploadId)
        => Path.Combine(GetSessionPath(uploadId), "chunks");

    private string GetMetadataPath(string uploadId)
        => Path.Combine(GetSessionPath(uploadId), "metadata.json");

    private string GetChunkPath(string uploadId, int chunkIndex)
        => Path.Combine(GetChunksPath(uploadId), $"{chunkIndex:000000}.part");
}