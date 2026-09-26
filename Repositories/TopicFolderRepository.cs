using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public class TopicFolderRepository : ITopicFolderRepository
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public TopicFolderRepository(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    public async Task<(IReadOnlyList<TopicFolder> Items, int TotalCount)> GetAllAsync(TopicFolderQueryParameters query)
    {
        var folders = _readDb.TopicFolders.AsQueryable();

        if (query.TopicId.HasValue)
            folders = folders.Where(f => f.TopicId == query.TopicId.Value);

        var totalCount = await folders.CountAsync();

        var items = await folders
            .OrderBy(f => f.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<List<TopicFolder>> GetAllByTopicAsync(Guid topicId)
        => _readDb.TopicFolders.Where(f => f.TopicId == topicId).ToListAsync();

    public Task<TopicFolder?> GetByIdAsync(Guid id)
        => _readDb.TopicFolders.FirstOrDefaultAsync(f => f.Id == id);

    public Task<TopicFolder?> GetForUpdateAsync(Guid id)
        => _writeDb.TopicFolders.FirstOrDefaultAsync(f => f.Id == id);

    public async Task AddAsync(TopicFolder folder)
        => await _writeDb.TopicFolders.AddAsync(folder);

    public void Remove(TopicFolder folder)
        => _writeDb.TopicFolders.Remove(folder);

    public Task SaveChangesAsync()
        => _writeDb.SaveChangesAsync();
}
