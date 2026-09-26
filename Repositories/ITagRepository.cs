using learnflow_service.Models;

namespace learnflow_service.Repositories;

public interface ITagRepository
{
    Task<List<(Tag Tag, int DocumentCount)>> GetAllWithCountsAsync();
    Task<List<Tag>> ResolveTagsAsync(IEnumerable<string> tagNames);
    Task SaveChangesAsync();
}
