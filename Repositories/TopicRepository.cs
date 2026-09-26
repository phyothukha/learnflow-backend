using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public TopicRepository(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    public async Task<(IReadOnlyList<Topic> Items, int TotalCount)> GetAllAsync(TopicQueryParameters query)
    {
        var topics = _readDb.Topics.AsQueryable();

        if (query.IsArchived.HasValue)
            topics = topics.Where(t => t.IsArchived == query.IsArchived.Value);

        var totalCount = await topics.CountAsync();

        var items = await topics
            .OrderBy(t => t.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Topic?> GetByIdAsync(Guid id)
        => _readDb.Topics.FirstOrDefaultAsync(t => t.Id == id);

    public Task<Topic?> GetForUpdateAsync(Guid id)
        => _writeDb.Topics.FirstOrDefaultAsync(t => t.Id == id);

    public async Task AddAsync(Topic topic)
        => await _writeDb.Topics.AddAsync(topic);

    public void Remove(Topic topic)
        => _writeDb.Topics.Remove(topic);

    public Task SaveChangesAsync()
        => _writeDb.SaveChangesAsync();
}
