using Mapster;
using MapsterMapper;
using learnflow_service.Dtos;
using learnflow_service.Models;
using learnflow_service.Repositories;

namespace learnflow_service.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly ITagRepository _tagRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;

    public DocumentService(
        IDocumentRepository repository,
        ITagRepository tagRepository,
        IFileStorageService fileStorage,
        IMapper mapper)
    {
        _repository = repository;
        _tagRepository = tagRepository;
        _fileStorage = fileStorage;
        _mapper = mapper;
    }

    public async Task<PagedResult<DocumentResponse>> GetAllAsync(DocumentQueryParameters query)
    {
        var (items, totalCount) = await _repository.GetAllAsync(query);

        return new PagedResult<DocumentResponse>
        {
            Items = _mapper.Map<List<DocumentResponse>>(items),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<DocumentResponse?> GetByIdAsync(Guid id)
    {
        var document = await _repository.GetByIdAsync(id);
        return document == null ? null : _mapper.Map<DocumentResponse>(document);
    }

    public async Task<DocumentResponse> CreateAsync(CreateDocumentRequest request)
    {
        var tags = await _tagRepository.ResolveTagsAsync(request.Tags);

        var document = request.Adapt<Document>();
        document.DocumentTags = tags
            .Select(t => new DocumentTag { DocumentId = document.Id, TagId = t.Id, Tag = t })
            .ToList();

        await _repository.AddAsync(document);
        await _repository.SaveChangesAsync();

        return _mapper.Map<DocumentResponse>(document);
    }

    public async Task<DocumentResponse?> UpdateAsync(Guid id, UpdateDocumentRequest request)
    {
        var document = await _repository.GetForUpdateAsync(id);
        if (document == null) return null;

        _mapper.Map(request, document);

        if (request.Tags != null)
        {
            var tags = await _tagRepository.ResolveTagsAsync(request.Tags);
            document.DocumentTags.Clear();
            foreach (var tag in tags)
                document.DocumentTags.Add(new DocumentTag { DocumentId = document.Id, TagId = tag.Id, Tag = tag });
        }

        document.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return _mapper.Map<DocumentResponse>(document);
    }

    public async Task<DocumentResponse?> MoveAsync(Guid id, MoveDocumentRequest request)
    {
        var document = await _repository.GetForUpdateAsync(id);
        if (document == null) return null;

        document.FolderId = request.FolderId;
        document.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return _mapper.Map<DocumentResponse>(document);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var document = await _repository.GetForUpdateAsync(id);
        if (document == null) return false;

        foreach (var attachment in document.Attachments)
            await _fileStorage.DeleteAsync(attachment.StoragePath);

        _repository.Remove(document);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<AttachmentUploadResult> AddAttachmentAsync(Guid documentId, Stream content, string fileName, string contentType, long sizeBytes)
    {
        if (!_fileStorage.IsConfigured)
            return new AttachmentUploadResult(AttachmentUploadStatus.StorageNotConfigured, null);

        var document = await _repository.GetForUpdateAsync(documentId);
        if (document == null)
            return new AttachmentUploadResult(AttachmentUploadStatus.DocumentNotFound, null);

        var uploaded = await _fileStorage.UploadAsync(content, fileName, contentType);

        var attachment = new Attachment
        {
            DocumentId = documentId,
            FileName = fileName,
            StorageUrl = uploaded.PublicUrl,
            StoragePath = uploaded.StoragePath,
            ContentType = contentType,
            SizeBytes = sizeBytes
        };

        await _repository.AddAttachmentAsync(attachment);
        await _repository.SaveChangesAsync();

        return new AttachmentUploadResult(AttachmentUploadStatus.Success, _mapper.Map<AttachmentResponse>(attachment));
    }

    public async Task<bool> DeleteAttachmentAsync(Guid documentId, Guid attachmentId)
    {
        var attachment = await _repository.GetAttachmentAsync(attachmentId);
        if (attachment == null || attachment.DocumentId != documentId) return false;

        await _fileStorage.DeleteAsync(attachment.StoragePath);
        _repository.RemoveAttachment(attachment);
        await _repository.SaveChangesAsync();

        return true;
    }
}
