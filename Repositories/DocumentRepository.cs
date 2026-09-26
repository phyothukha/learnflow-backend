using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public DocumentRepository(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    private static IQueryable<Document> IncludeRelated(IQueryable<Document> documents)
        => documents
            .Include(d => d.DocumentTags).ThenInclude(dt => dt.Tag)
            .Include(d => d.Attachments);

    public async Task<(IReadOnlyList<Document> Items, int TotalCount)> GetAllAsync(DocumentQueryParameters query)
    {
        var documents = IncludeRelated(_readDb.Documents.AsQueryable());

        if (query.TopicId.HasValue)
            documents = documents.Where(d => d.TopicId == query.TopicId.Value);

        if (query.FolderId.HasValue)
            documents = documents.Where(d => d.FolderId == query.FolderId.Value);

        if (query.Status.HasValue)
            documents = documents.Where(d => d.Status == query.Status.Value);

        if (!string.IsNullOrWhiteSpace(query.Tag))
            documents = documents.Where(d => d.DocumentTags.Any(dt => dt.Tag.Name == query.Tag));

        if (!string.IsNullOrWhiteSpace(query.Search))
            documents = documents.Where(d => d.Title.Contains(query.Search));

        var totalCount = await documents.CountAsync();

        var items = await documents
            .OrderByDescending(d => d.UpdatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Document?> GetByIdAsync(Guid id)
        => IncludeRelated(_readDb.Documents.AsQueryable()).FirstOrDefaultAsync(d => d.Id == id);

    public Task<Document?> GetForUpdateAsync(Guid id)
        => IncludeRelated(_writeDb.Documents.AsQueryable()).FirstOrDefaultAsync(d => d.Id == id);

    public async Task AddAsync(Document document)
        => await _writeDb.Documents.AddAsync(document);

    public void Remove(Document document)
        => _writeDb.Documents.Remove(document);

    public Task SaveChangesAsync()
        => _writeDb.SaveChangesAsync();

    public async Task AddAttachmentAsync(Attachment attachment)
        => await _writeDb.Attachments.AddAsync(attachment);

    public Task<Attachment?> GetAttachmentAsync(Guid attachmentId)
        => _writeDb.Attachments.FirstOrDefaultAsync(a => a.Id == attachmentId);

    public void RemoveAttachment(Attachment attachment)
        => _writeDb.Attachments.Remove(attachment);
}
