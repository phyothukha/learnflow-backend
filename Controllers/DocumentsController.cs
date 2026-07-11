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

public class DocumentsController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public DocumentsController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<Document> Get() => _readDb.Documents;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<Document> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.Documents.Where(d => d.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Document document)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.Documents.Add(document);
        await _writeDb.SaveChangesAsync();
        return Created(document);
    }

    [EnableQuery]
    public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<Document> delta)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _writeDb.Documents.FindAsync(key);
        if (existing == null) return NotFound();

        delta.Patch(existing);
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _writeDb.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_writeDb.Documents.Any(d => d.Id == key)) return NotFound();
            throw;
        }

        return Updated(existing);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.Documents.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.Documents.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
