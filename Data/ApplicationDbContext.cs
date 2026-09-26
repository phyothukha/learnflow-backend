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
    public DbSet<Topic> Topics { get; set; }
    public DbSet<TopicFolder> TopicFolders { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<StudyBlock> StudyBlocks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<DocumentTag> DocumentTags { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

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

        builder.Entity<Topic>(e =>
        {
            e.ToTable("Topics");
            e.HasMany(x => x.Folders).WithOne(f => f.Topic).HasForeignKey(f => f.TopicId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Documents).WithOne(d => d.Topic).HasForeignKey(d => d.TopicId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Notes).WithOne(n => n.Topic).HasForeignKey(n => n.TopicId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.StudyBlocks).WithOne(b => b.Topic).HasForeignKey(b => b.TopicId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<TopicFolder>(e =>
        {
            e.ToTable("TopicFolders");
            e.HasIndex(x => x.TopicId);
            e.HasMany(x => x.Children).WithOne(f => f.ParentFolder).HasForeignKey(f => f.ParentFolderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Document>(e =>
        {
            e.ToTable("Documents");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => new { x.TopicId, x.Status });
            e.HasOne(x => x.Folder).WithMany(f => f.Documents).HasForeignKey(x => x.FolderId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Attachments).WithOne(a => a.Document).HasForeignKey(a => a.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Tag>(e =>
        {
            e.ToTable("Tags");
            e.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<DocumentTag>(e =>
        {
            e.ToTable("DocumentTags");
            e.HasKey(x => new { x.DocumentId, x.TagId });
            e.HasOne(x => x.Document).WithMany(d => d.DocumentTags).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tag).WithMany(t => t.DocumentTags).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Attachment>(e =>
        {
            e.ToTable("Attachments");
            e.HasIndex(x => x.DocumentId);
        });

        builder.Entity<Note>(e =>
        {
            e.ToTable("Notes");
            e.HasIndex(x => x.TopicId);
            e.HasOne(x => x.Document).WithMany(d => d.Notes).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StudyBlock>(e =>
        {
            e.ToTable("StudyBlocks");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => x.StartAt);
        });
    }
}
