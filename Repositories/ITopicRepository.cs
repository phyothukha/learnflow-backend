using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public interface ITopicRepository
{
    Task<(IReadOnlyList<Topic> Items, int TotalCount)> GetAllAsync(TopicQueryParameters query);
    Task<Topic?> GetByIdAsync(Guid id);
    Task<Topic?> GetForUpdateAsync(Guid id);
    Task AddAsync(Topic topic);
    void Remove(Topic topic);
    Task SaveChangesAsync();
}
