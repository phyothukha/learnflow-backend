using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public TagRepository(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    public async Task<List<(Tag Tag, int DocumentCount)>> GetAllWithCountsAsync()
    {
        var tags = await _readDb.Tags
            .Select(t => new { Tag = t, DocumentCount = t.DocumentTags.Count })
            .OrderBy(t => t.Tag.Name)
            .ToListAsync();

        return tags.Select(t => (t.Tag, t.DocumentCount)).ToList();
    }

    public async Task<List<Tag>> ResolveTagsAsync(IEnumerable<string> tagNames)
    {
        var normalizedNames = tagNames
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedNames.Count == 0) return new List<Tag>();

        var existing = await _writeDb.Tags
            .Where(t => normalizedNames.Contains(t.Name))
            .ToListAsync();

        var existingNames = existing.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newTags = normalizedNames
            .Where(n => !existingNames.Contains(n))
            .Select(n => new Tag { Name = n })
            .ToList();

        if (newTags.Count > 0)
            await _writeDb.Tags.AddRangeAsync(newTags);

        return existing.Concat(newTags).ToList();
    }

    public Task SaveChangesAsync() => _writeDb.SaveChangesAsync();
}
