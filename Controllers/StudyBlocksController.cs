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

public class StudyBlocksController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public StudyBlocksController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<StudyBlock> Get() => _readDb.StudyBlocks;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<StudyBlock> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.StudyBlocks.Where(b => b.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] StudyBlock block)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.StudyBlocks.Add(block);
        await _writeDb.SaveChangesAsync();
        return Created(block);
    }

    [EnableQuery]
    public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<StudyBlock> delta)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _writeDb.StudyBlocks.FindAsync(key);
        if (existing == null) return NotFound();

        delta.Patch(existing);
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _writeDb.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_writeDb.StudyBlocks.Any(b => b.Id == key)) return NotFound();
            throw;
        }

        return Updated(existing);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.StudyBlocks.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.StudyBlocks.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
