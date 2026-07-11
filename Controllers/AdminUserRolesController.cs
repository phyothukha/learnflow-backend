using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Models;

namespace learnflow_service.Controllers;

public class AdminUserRolesController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public AdminUserRolesController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<AdminUserRole> Get() => _readDb.AdminUserRoles;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<AdminUserRole> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.AdminUserRoles.Where(ur => ur.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AdminUserRole userRole)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.AdminUserRoles.Add(userRole);
        await _writeDb.SaveChangesAsync();
        return Created(userRole);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.AdminUserRoles.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.AdminUserRoles.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
