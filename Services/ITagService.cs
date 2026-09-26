using learnflow_service.Dtos;

namespace learnflow_service.Services;

public interface ITagService
{
    Task<List<TagResponse>> GetAllAsync();
}
