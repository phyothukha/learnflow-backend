using Mapster;
using MapsterMapper;
using learnflow_service.Dtos;
using learnflow_service.Models;
using learnflow_service.Repositories;

namespace learnflow_service.Services;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _repository;
    private readonly IMapper _mapper;

    public TopicService(ITopicRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TopicResponse>> GetAllAsync(TopicQueryParameters query)
    {
        var (items, totalCount) = await _repository.GetAllAsync(query);

        return new PagedResult<TopicResponse>
        {
            Items = _mapper.Map<List<TopicResponse>>(items),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TopicResponse?> GetByIdAsync(Guid id)
    {
        var topic = await _repository.GetByIdAsync(id);
        return topic == null ? null : _mapper.Map<TopicResponse>(topic);
    }

    public async Task<TopicResponse> CreateAsync(CreateTopicRequest request)
    {
        var topic = request.Adapt<Topic>();

        await _repository.AddAsync(topic);
        await _repository.SaveChangesAsync();

        return _mapper.Map<TopicResponse>(topic);
    }

    public async Task<TopicResponse?> UpdateAsync(Guid id, UpdateTopicRequest request)
    {
        var topic = await _repository.GetForUpdateAsync(id);
        if (topic == null) return null;

        _mapper.Map(request, topic);
        topic.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return _mapper.Map<TopicResponse>(topic);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var topic = await _repository.GetForUpdateAsync(id);
        if (topic == null) return false;

        _repository.Remove(topic);
        await _repository.SaveChangesAsync();

        return true;
    }
}
