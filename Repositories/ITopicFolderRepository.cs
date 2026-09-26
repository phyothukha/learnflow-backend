using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public interface ITopicFolderRepository
{
    Task<(IReadOnlyList<TopicFolder> Items, int TotalCount)> GetAllAsync(TopicFolderQueryParameters query);
    Task<List<TopicFolder>> GetAllByTopicAsync(Guid topicId);
    Task<TopicFolder?> GetByIdAsync(Guid id);
    Task<TopicFolder?> GetForUpdateAsync(Guid id);
    Task AddAsync(TopicFolder folder);
    void Remove(TopicFolder folder);
    Task SaveChangesAsync();
}
