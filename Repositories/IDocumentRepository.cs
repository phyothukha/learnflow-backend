using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public interface IDocumentRepository
{
    Task<(IReadOnlyList<Document> Items, int TotalCount)> GetAllAsync(DocumentQueryParameters query);
    Task<Document?> GetByIdAsync(Guid id);
    Task<Document?> GetForUpdateAsync(Guid id);
    Task AddAsync(Document document);
    void Remove(Document document);
    Task SaveChangesAsync();

    Task AddAttachmentAsync(Attachment attachment);
    Task<Attachment?> GetAttachmentAsync(Guid attachmentId);
    void RemoveAttachment(Attachment attachment);
}
