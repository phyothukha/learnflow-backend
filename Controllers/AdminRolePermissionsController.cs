using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.DependencyInjection;
using learnflow_service.Models;

namespace learnflow_service.Controllers;

public class AdminRolePermissionsController : ODataController
{
    private readonly ApplicationDbContext _readDb;
    private readonly ApplicationDbContext _writeDb;

    public AdminRolePermissionsController(
        [FromKeyedServices("read")] ApplicationDbContext readDb,
        [FromKeyedServices("write")] ApplicationDbContext writeDb)
    {
        _readDb = readDb;
        _writeDb = writeDb;
    }

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public IQueryable<AdminRolePermission> Get() => _readDb.AdminRolePermissions;

    [EnableQuery(PageSize = 100, MaxExpansionDepth = 10)]
    public SingleResult<AdminRolePermission> Get([FromODataUri] Guid key)
        => SingleResult.Create(_readDb.AdminRolePermissions.Where(rp => rp.Id == key));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AdminRolePermission rolePermission)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _writeDb.AdminRolePermissions.Add(rolePermission);
        await _writeDb.SaveChangesAsync();
        return Created(rolePermission);
    }

    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] Guid key)
    {
        var existing = await _writeDb.AdminRolePermissions.FindAsync(key);
        if (existing == null) return NotFound();

        _writeDb.AdminRolePermissions.Remove(existing);
        await _writeDb.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
