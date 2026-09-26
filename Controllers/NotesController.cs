using Microsoft.AspNetCore.Mvc;
using learnflow_service.Dtos;
using learnflow_service.Services;

namespace learnflow_service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<NoteResponse>>> Get([FromQuery] NoteQueryParameters query)
        => Ok(await _noteService.GetAllAsync(query));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NoteResponse>> Get(Guid id)
    {
        var note = await _noteService.GetByIdAsync(id);
        return note == null ? NotFound() : Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<NoteResponse>> Post([FromBody] CreateNoteRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _noteService.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<NoteResponse>> Patch(Guid id, [FromBody] UpdateNoteRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _noteService.UpdateAsync(id, request);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _noteService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
