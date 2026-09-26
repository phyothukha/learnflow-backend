using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public interface ICourseRepository
{
    Task<(IReadOnlyList<Course> Items, int TotalCount)> GetAllAsync(CourseQueryParameters query);
    Task<Course?> GetByIdAsync(Guid id);
    Task<Course?> GetForUpdateAsync(Guid id);
    Task AddAsync(Course course);
    void Remove(Course course);
    Task SaveChangesAsync();
}
