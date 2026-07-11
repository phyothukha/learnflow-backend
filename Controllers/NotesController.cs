using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Models;

namespace learnflow_service.Controllers;

public class NotesController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public NotesController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<Note> Get() => _readDb.Notes;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<Note> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.Notes.Where(n => n.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Note note)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.Notes.Add(note);
        await _writeDb.SaveChangesAsync();
        return Created(note);
    }

    [EnableQuery]
    public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<Note> delta)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _writeDb.Notes.FindAsync(key);
        if (existing == null) return NotFound();

        delta.Patch(existing);
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _writeDb.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_writeDb.Notes.Any(n => n.Id == key)) return NotFound();
            throw;
        }

        return Updated(existing);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.Notes.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.Notes.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
