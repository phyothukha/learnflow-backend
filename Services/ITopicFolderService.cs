using learnflow_service.Dtos;

namespace learnflow_service.Services;

public interface ITopicFolderService
{
    Task<PagedResult<TopicFolderResponse>> GetAllAsync(TopicFolderQueryParameters query);
    Task<TopicFolderResponse?> GetByIdAsync(Guid id);
    Task<List<TopicFolderTreeNode>> GetTreeAsync(Guid topicId);
    Task<TopicFolderResponse> CreateAsync(CreateTopicFolderRequest request);
    Task<TopicFolderResponse?> UpdateAsync(Guid id, UpdateTopicFolderRequest request);
    Task<(MoveFolderResult Result, TopicFolderResponse? Folder)> MoveAsync(Guid id, MoveTopicFolderRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public enum MoveFolderResult
{
    Success,
    NotFound,
    InvalidParent,
    WouldCreateCycle
}
