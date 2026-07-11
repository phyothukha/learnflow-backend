using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("AdminPermissions")]
public class AdminPermission : BaseModel
{
    [Required, MaxLength(100)]
    public string Resource { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Action { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<AdminRolePermission> RolePermissions { get; set; } = new List<AdminRolePermission>();
}
