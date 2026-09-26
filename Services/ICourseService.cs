using learnflow_service.Dtos;

namespace learnflow_service.Services;

public interface ICourseService
{
    Task<PagedResult<CourseResponse>> GetAllAsync(CourseQueryParameters query);
    Task<CourseResponse?> GetByIdAsync(Guid id);
    Task<CourseResponse> CreateAsync(CreateCourseRequest request);
    Task<CourseResponse?> UpdateAsync(Guid id, UpdateCourseRequest request);
    Task<bool> DeleteAsync(Guid id);
}
