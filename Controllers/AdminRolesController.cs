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

public class AdminRolesController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public AdminRolesController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<AdminRole> Get() => _readDb.AdminRoles;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<AdminRole> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.AdminRoles.Where(r => r.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AdminRole role)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.AdminRoles.Add(role);
        await _writeDb.SaveChangesAsync();
        return Created(role);
    }

    [EnableQuery]
    public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<AdminRole> delta)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _writeDb.AdminRoles.FindAsync(key);
        if (existing == null) return NotFound();

        delta.Patch(existing);

        try
        {
            await _writeDb.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_writeDb.AdminRoles.Any(r => r.Id == key)) return NotFound();
            throw;
        }

        return Updated(existing);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.AdminRoles.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.AdminRoles.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
