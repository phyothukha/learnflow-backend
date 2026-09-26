using Mapster;
using MapsterMapper;
using learnflow_service.Dtos;
using learnflow_service.Models;
using learnflow_service.Repositories;

namespace learnflow_service.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseResponse>> GetAllAsync(CourseQueryParameters query)
    {
        var (items, totalCount) = await _repository.GetAllAsync(query);

        return new PagedResult<CourseResponse>
        {
            Items = _mapper.Map<List<CourseResponse>>(items),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CourseResponse?> GetByIdAsync(Guid id)
    {
        var course = await _repository.GetByIdAsync(id);
        return course == null ? null : _mapper.Map<CourseResponse>(course);
    }

    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request)
    {
        var course = request.Adapt<Course>();

        await _repository.AddAsync(course);
        await _repository.SaveChangesAsync();

        return _mapper.Map<CourseResponse>(course);
    }

    public async Task<CourseResponse?> UpdateAsync(Guid id, UpdateCourseRequest request)
    {
        var course = await _repository.GetForUpdateAsync(id);
        if (course == null) return null;

        _mapper.Map(request, course);
        course.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return _mapper.Map<CourseResponse>(course);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var course = await _repository.GetForUpdateAsync(id);
        if (course == null) return false;

        _repository.Remove(course);
        await _repository.SaveChangesAsync();

        return true;
    }
}
