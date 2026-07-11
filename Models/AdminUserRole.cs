using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("AdminUserRoles")]
public class AdminUserRole
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("RoleId")]
    public Guid RoleId { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AdminUser User { get; set; } = null!;

    [ForeignKey(nameof(RoleId))]
    public AdminRole Role { get; set; } = null!;
}
