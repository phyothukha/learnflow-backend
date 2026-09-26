using MapsterMapper;
using learnflow_service.Dtos;
using learnflow_service.Repositories;

namespace learnflow_service.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _repository;
    private readonly IMapper _mapper;

    public TagService(ITagRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<TagResponse>> GetAllAsync()
    {
        var tags = await _repository.GetAllWithCountsAsync();

        return tags.Select(t =>
        {
            var response = _mapper.Map<TagResponse>(t.Tag);
            response.DocumentCount = t.DocumentCount;
            return response;
        }).ToList();
    }
}
