using learnflow_service.Dtos;

namespace learnflow_service.Services;

public enum AttachmentUploadStatus
{
    Success,
    DocumentNotFound,
    StorageNotConfigured
}

public record AttachmentUploadResult(AttachmentUploadStatus Status, AttachmentResponse? Attachment);

public interface IDocumentService
{
    Task<PagedResult<DocumentResponse>> GetAllAsync(DocumentQueryParameters query);
    Task<DocumentResponse?> GetByIdAsync(Guid id);
    Task<DocumentResponse> CreateAsync(CreateDocumentRequest request);
    Task<DocumentResponse?> UpdateAsync(Guid id, UpdateDocumentRequest request);
    Task<DocumentResponse?> MoveAsync(Guid id, MoveDocumentRequest request);
    Task<bool> DeleteAsync(Guid id);

    Task<AttachmentUploadResult> AddAttachmentAsync(Guid documentId, Stream content, string fileName, string contentType, long sizeBytes);
    Task<bool> DeleteAttachmentAsync(Guid documentId, Guid attachmentId);
}
