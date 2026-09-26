using Mapster;
using MapsterMapper;
using learnflow_service.Dtos;
using learnflow_service.Models;
using learnflow_service.Repositories;

namespace learnflow_service.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _repository;
    private readonly IMapper _mapper;

    public NoteService(INoteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<NoteResponse>> GetAllAsync(NoteQueryParameters query)
    {
        var (items, totalCount) = await _repository.GetAllAsync(query);

        return new PagedResult<NoteResponse>
        {
            Items = _mapper.Map<List<NoteResponse>>(items),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<NoteResponse?> GetByIdAsync(Guid id)
    {
        var note = await _repository.GetByIdAsync(id);
        return note == null ? null : _mapper.Map<NoteResponse>(note);
    }

    public async Task<NoteResponse> CreateAsync(CreateNoteRequest request)
    {
        var note = request.Adapt<Note>();

        await _repository.AddAsync(note);
        await _repository.SaveChangesAsync();

        return _mapper.Map<NoteResponse>(note);
    }

    public async Task<NoteResponse?> UpdateAsync(Guid id, UpdateNoteRequest request)
    {
        var note = await _repository.GetForUpdateAsync(id);
        if (note == null) return null;

        _mapper.Map(request, note);
        note.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return _mapper.Map<NoteResponse>(note);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var note = await _repository.GetForUpdateAsync(id);
        if (note == null) return false;

        _repository.Remove(note);
        await _repository.SaveChangesAsync();

        return true;
    }
}
