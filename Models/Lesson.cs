using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("Lessons")]
public class Lesson : BaseModel
{
    [Column("CourseId")]
    public Guid CourseId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    public int Order { get; set; }

    public TimeSpan? Duration { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;
}
