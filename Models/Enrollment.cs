using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

public enum EnrollmentStatus
{
    Pending,
    Active,
    Completed,
    Cancelled
}

[Table("Enrollments")]
public class Enrollment : BaseModel
{
    [Column("CourseId")]
    public Guid CourseId { get; set; }

    [Required, MaxLength(255)]
    public string StudentEmail { get; set; } = null!;

    [Required, MaxLength(255)]
    public string StudentName { get; set; } = null!;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

    public int ProgressPercent { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;
}
