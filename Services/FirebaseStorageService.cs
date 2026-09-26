using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

namespace learnflow_service.Services;

public class FirebaseStorageService : IFileStorageService
{
    private readonly StorageClient? _client;
    private readonly string? _bucket;

    public FirebaseStorageService(StorageClient? client, string? bucket)
    {
        _client = client;
        _bucket = bucket;
    }

    public bool IsConfigured => _client != null && !string.IsNullOrEmpty(_bucket);

    public async Task<UploadedFile> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("File storage is not configured.");

        var storagePath = $"attachments/{Guid.NewGuid()}/{fileName}";

        var obj = await _client!.UploadObjectAsync(
            new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = _bucket,
                Name = storagePath,
                ContentType = contentType
            },
            content,
            new UploadObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead },
            cancellationToken: ct);

        var publicUrl = $"https://storage.googleapis.com/{_bucket}/{Uri.EscapeDataString(storagePath)}";
        return new UploadedFile(storagePath, publicUrl);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return Task.CompletedTask;

        return _client!.DeleteObjectAsync(_bucket, storagePath, cancellationToken: ct);
    }
}
