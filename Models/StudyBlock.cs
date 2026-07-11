using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

public enum StudyBlockStatus
{
    Upcoming,
    Active,
    Done,
    Missed
}

[Table("StudyBlocks")]
public class StudyBlock : BaseModel
{
    public Guid? TopicId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public StudyBlockStatus Status { get; set; } = StudyBlockStatus.Upcoming;

    public int ReminderMinutesBefore { get; set; } = 5;

    [MaxLength(255)]
    public string? RecurrenceRule { get; set; }

    [ForeignKey(nameof(TopicId))]
    public Topic? Topic { get; set; }
}
