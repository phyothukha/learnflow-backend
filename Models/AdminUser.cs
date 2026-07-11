using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("AdminUsers")]
public class AdminUser : BaseModel
{
    [Required, MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(255)]
    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = null!;

    [Required, MaxLength(255)]
    public string Name { get; set; } = null!;

    [Column("IsActive")]
    public bool? IsActive { get; set; }

    public ICollection<AdminUserRole> UserRoles { get; set; } = new List<AdminUserRole>();
}
