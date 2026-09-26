using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public interface INoteRepository
{
    Task<(IReadOnlyList<Note> Items, int TotalCount)> GetAllAsync(NoteQueryParameters query);
    Task<Note?> GetByIdAsync(Guid id);
    Task<Note?> GetForUpdateAsync(Guid id);
    Task AddAsync(Note note);
    void Remove(Note note);
    Task SaveChangesAsync();
}
