using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("AdminRolePermissions")]
public class AdminRolePermission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("RoleId")]
    public Guid RoleId { get; set; }

    [Column("PermissionId")]
    public Guid PermissionId { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RoleId))]
    public AdminRole Role { get; set; } = null!;

    [ForeignKey(nameof(PermissionId))]
    public AdminPermission Permission { get; set; } = null!;
}
