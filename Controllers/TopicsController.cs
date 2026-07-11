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

public class TopicsController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public TopicsController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<Topic> Get() => _readDb.Topics;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<Topic> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.Topics.Where(t => t.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Topic topic)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.Topics.Add(topic);
        await _writeDb.SaveChangesAsync();
        return Created(topic);
    }

    [EnableQuery]
    public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<Topic> delta)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _writeDb.Topics.FindAsync(key);
        if (existing == null) return NotFound();

        delta.Patch(existing);
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _writeDb.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_writeDb.Topics.Any(t => t.Id == key)) return NotFound();
            throw;
        }

        return Updated(existing);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.Topics.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.Topics.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
