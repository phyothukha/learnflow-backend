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

public class CoursesController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public CoursesController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<Course> Get() => _readDb.Courses;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<Course> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.Courses.Where(c => c.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Course course)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.Courses.Add(course);
        await _writeDb.SaveChangesAsync();
        return Created(course);
    }

    [EnableQuery]
    public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<Course> delta)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _writeDb.Courses.FindAsync(key);
        if (existing == null) return NotFound();

        delta.Patch(existing);
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _writeDb.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_writeDb.Courses.Any(c => c.Id == key)) return NotFound();
            throw;
        }

        return Updated(existing);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.Courses.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.Courses.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
