using Microsoft.EntityFrameworkCore;
using learnflow_service.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<AdminRole> AdminRoles { get; set; }
    public DbSet<AdminPermission> AdminPermissions { get; set; }
    public DbSet<AdminRolePermission> AdminRolePermissions { get; set; }
    public DbSet<AdminUserRole> AdminUserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>(e =>
        {
            e.ToTable("Courses");
            e.HasMany(x => x.Lessons).WithOne(l => l.Course).HasForeignKey(l => l.CourseId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Enrollments).WithOne(en => en.Course).HasForeignKey(en => en.CourseId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Lesson>(e =>
        {
            e.ToTable("Lessons");
            e.HasIndex(x => new { x.CourseId, x.Order });
        });

        builder.Entity<Enrollment>(e =>
        {
            e.ToTable("Enrollments");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => new { x.CourseId, x.StudentEmail }).IsUnique();
        });

        builder.Entity<AdminUser>(e =>
        {
            e.ToTable("AdminUsers");
            e.HasIndex(x => x.Email).IsUnique();
            e.HasMany(x => x.UserRoles).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AdminRole>(e =>
        {
            e.ToTable("AdminRoles");
            e.HasMany(x => x.RolePermissions).WithOne(r => r.Role).HasForeignKey(r => r.RoleId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.UserRoles).WithOne(r => r.Role).HasForeignKey(r => r.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AdminPermission>(e =>
        {
            e.ToTable("AdminPermissions");
            e.HasMany(x => x.RolePermissions).WithOne(r => r.Permission).HasForeignKey(r => r.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AdminRolePermission>(e => e.ToTable("AdminRolePermissions"));
        builder.Entity<AdminUserRole>(e => e.ToTable("AdminUserRoles"));
    }
}
