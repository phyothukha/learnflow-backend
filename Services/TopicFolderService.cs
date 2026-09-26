using Mapster;
using MapsterMapper;
using learnflow_service.Dtos;
using learnflow_service.Models;
using learnflow_service.Repositories;

namespace learnflow_service.Services;

public class TopicFolderService : ITopicFolderService
{
    private readonly ITopicFolderRepository _repository;
    private readonly IMapper _mapper;

    public TopicFolderService(ITopicFolderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TopicFolderResponse>> GetAllAsync(TopicFolderQueryParameters query)
    {
        var (items, totalCount) = await _repository.GetAllAsync(query);

        return new PagedResult<TopicFolderResponse>
        {
            Items = _mapper.Map<List<TopicFolderResponse>>(items),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TopicFolderResponse?> GetByIdAsync(Guid id)
    {
        var folder = await _repository.GetByIdAsync(id);
        return folder == null ? null : _mapper.Map<TopicFolderResponse>(folder);
    }

    public async Task<List<TopicFolderTreeNode>> GetTreeAsync(Guid topicId)
    {
        var folders = await _repository.GetAllByTopicAsync(topicId);

        var nodesById = folders.ToDictionary(f => f.Id, f => _mapper.Map<TopicFolderTreeNode>(f));

        var roots = new List<TopicFolderTreeNode>();

        foreach (var folder in folders)
        {
            var node = nodesById[folder.Id];

            if (folder.ParentFolderId.HasValue && nodesById.TryGetValue(folder.ParentFolderId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return roots;
    }

    public async Task<TopicFolderResponse> CreateAsync(CreateTopicFolderRequest request)
    {
        var folder = request.Adapt<TopicFolder>();

        await _repository.AddAsync(folder);
        await _repository.SaveChangesAsync();

        return _mapper.Map<TopicFolderResponse>(folder);
    }

    public async Task<TopicFolderResponse?> UpdateAsync(Guid id, UpdateTopicFolderRequest request)
    {
        var folder = await _repository.GetForUpdateAsync(id);
        if (folder == null) return null;

        _mapper.Map(request, folder);
        folder.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return _mapper.Map<TopicFolderResponse>(folder);
    }

    public async Task<(MoveFolderResult Result, TopicFolderResponse? Folder)> MoveAsync(Guid id, MoveTopicFolderRequest request)
    {
        var folder = await _repository.GetForUpdateAsync(id);
        if (folder == null) return (MoveFolderResult.NotFound, null);

        if (request.ParentFolderId.HasValue)
        {
            if (request.ParentFolderId.Value == id)
                return (MoveFolderResult.WouldCreateCycle, null);

            var newParent = await _repository.GetForUpdateAsync(request.ParentFolderId.Value);
            if (newParent == null || newParent.TopicId != folder.TopicId)
                return (MoveFolderResult.InvalidParent, null);

            if (await IsDescendantAsync(folder.TopicId, request.ParentFolderId.Value, id))
                return (MoveFolderResult.WouldCreateCycle, null);
        }

        folder.ParentFolderId = request.ParentFolderId;
        folder.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return (MoveFolderResult.Success, _mapper.Map<TopicFolderResponse>(folder));
    }

    private async Task<bool> IsDescendantAsync(Guid topicId, Guid candidateId, Guid ancestorId)
    {
        var allFolders = await _repository.GetAllByTopicAsync(topicId);
        var byId = allFolders.ToDictionary(f => f.Id);

        var current = candidateId;
        while (byId.TryGetValue(current, out var node) && node.ParentFolderId.HasValue)
        {
            if (node.ParentFolderId.Value == ancestorId) return true;
            current = node.ParentFolderId.Value;
        }

        return false;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var folder = await _repository.GetForUpdateAsync(id);
        if (folder == null) return false;

        _repository.Remove(folder);
        await _repository.SaveChangesAsync();

        return true;
    }
}
