using learnflow_service.Dtos;
using learnflow_service.Models;
using learnflow_service.Repositories;

namespace learnflow_service.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;

    public CourseService(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<CourseResponse>> GetAllAsync(CourseQueryParameters query)
    {
        var (items, totalCount) = await _repository.GetAllAsync(query);

        return new PagedResult<CourseResponse>
        {
            Items = items.Select(ToResponse).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CourseResponse?> GetByIdAsync(Guid id)
    {
        var course = await _repository.GetByIdAsync(id);
        return course == null ? null : ToResponse(course);
    }

    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request)
    {
        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            CoverImageUrl = request.CoverImageUrl,
            IsPublished = request.IsPublished
        };

        await _repository.AddAsync(course);
        await _repository.SaveChangesAsync();

        return ToResponse(course);
    }

    public async Task<CourseResponse?> UpdateAsync(Guid id, UpdateCourseRequest request)
    {
        var course = await _repository.GetForUpdateAsync(id);
        if (course == null) return null;

        if (request.Title != null) course.Title = request.Title;
        if (request.Description != null) course.Description = request.Description;
        if (request.Category != null) course.Category = request.Category;
        if (request.CoverImageUrl != null) course.CoverImageUrl = request.CoverImageUrl;
        if (request.IsPublished.HasValue) course.IsPublished = request.IsPublished.Value;
        course.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return ToResponse(course);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var course = await _repository.GetForUpdateAsync(id);
        if (course == null) return false;

        _repository.Remove(course);
        await _repository.SaveChangesAsync();

        return true;
    }

    private static CourseResponse ToResponse(Course course) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        Category = course.Category,
        CoverImageUrl = course.CoverImageUrl,
        IsPublished = course.IsPublished,
        CreatedAt = course.CreatedAt,
        UpdatedAt = course.UpdatedAt
    };
}
