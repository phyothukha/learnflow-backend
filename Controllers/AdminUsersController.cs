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

public class AdminUsersController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public AdminUsersController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<AdminUser> Get() => _readDb.AdminUsers;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<AdminUser> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.AdminUsers.Where(u => u.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AdminUser user)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.AdminUsers.Add(user);
        await _writeDb.SaveChangesAsync();
        return Created(user);
    }

    [EnableQuery]
    public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<AdminUser> delta)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _writeDb.AdminUsers.FindAsync(key);
        if (existing == null) return NotFound();

        delta.Patch(existing);

        try
        {
            await _writeDb.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_writeDb.AdminUsers.Any(u => u.Id == key)) return NotFound();
            throw;
        }

        return Updated(existing);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.AdminUsers.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.AdminUsers.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
