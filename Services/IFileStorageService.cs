namespace learnflow_service.Services;

public record UploadedFile(string StoragePath, string PublicUrl);

public interface IFileStorageService
{
    bool IsConfigured { get; }
    Task<UploadedFile> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
}
