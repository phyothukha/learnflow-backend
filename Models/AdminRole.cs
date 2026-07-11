using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("AdminRoles")]
public class AdminRole : BaseModel
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<AdminRolePermission> RolePermissions { get; set; } = new List<AdminRolePermission>();
    public ICollection<AdminUserRole> UserRoles { get; set; } = new List<AdminUserRole>();
}
