using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Dtos;
using learnflow_service.Models;

namespace learnflow_service.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public NoteRepository(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    public async Task<(IReadOnlyList<Note> Items, int TotalCount)> GetAllAsync(NoteQueryParameters query)
    {
        var notes = _readDb.Notes.AsQueryable();

        if (query.TopicId.HasValue)
            notes = notes.Where(n => n.TopicId == query.TopicId.Value);

        if (query.DocumentId.HasValue)
            notes = notes.Where(n => n.DocumentId == query.DocumentId.Value);

        var totalCount = await notes.CountAsync();

        var items = await notes
            .OrderByDescending(n => n.UpdatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Note?> GetByIdAsync(Guid id)
        => _readDb.Notes.FirstOrDefaultAsync(n => n.Id == id);

    public Task<Note?> GetForUpdateAsync(Guid id)
        => _writeDb.Notes.FirstOrDefaultAsync(n => n.Id == id);

    public async Task AddAsync(Note note)
        => await _writeDb.Notes.AddAsync(note);

    public void Remove(Note note)
        => _writeDb.Notes.Remove(note);

    public Task SaveChangesAsync()
        => _writeDb.SaveChangesAsync();
}
