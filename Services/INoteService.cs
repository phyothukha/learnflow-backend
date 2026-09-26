using learnflow_service.Dtos;

namespace learnflow_service.Services;

public interface INoteService
{
    Task<PagedResult<NoteResponse>> GetAllAsync(NoteQueryParameters query);
    Task<NoteResponse?> GetByIdAsync(Guid id);
    Task<NoteResponse> CreateAsync(CreateNoteRequest request);
    Task<NoteResponse?> UpdateAsync(Guid id, UpdateNoteRequest request);
    Task<bool> DeleteAsync(Guid id);
}
