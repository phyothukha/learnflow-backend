using learnflow_service.Dtos;

namespace learnflow_service.Services;

public interface ITopicService
{
    Task<PagedResult<TopicResponse>> GetAllAsync(TopicQueryParameters query);
    Task<TopicResponse?> GetByIdAsync(Guid id);
    Task<TopicResponse> CreateAsync(CreateTopicRequest request);
    Task<TopicResponse?> UpdateAsync(Guid id, UpdateTopicRequest request);
    Task<bool> DeleteAsync(Guid id);
}
