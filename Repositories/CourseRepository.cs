using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public CourseRepository(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    public async Task<(IReadOnlyList<Course> Items, int TotalCount)> GetAllAsync(CourseQueryParameters query)
    {
        var courses = _readDb.Courses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Category))
            courses = courses.Where(c => c.Category == query.Category);

        if (query.IsPublished.HasValue)
            courses = courses.Where(c => c.IsPublished == query.IsPublished.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
            courses = courses.Where(c => c.Title.Contains(query.Search));

        var totalCount = await courses.CountAsync();

        var items = await courses
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Course?> GetByIdAsync(Guid id)
        => _readDb.Courses.FirstOrDefaultAsync(c => c.Id == id);

    public Task<Course?> GetForUpdateAsync(Guid id)
        => _writeDb.Courses.FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Course course)
        => await _writeDb.Courses.AddAsync(course);

    public void Remove(Course course)
        => _writeDb.Courses.Remove(course);

    public Task SaveChangesAsync()
        => _writeDb.SaveChangesAsync();
}
