using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

public enum DocumentStatus
{
    Unread,
    InProgress,
    Completed
}

[Table("Documents")]
public class Document : BaseModel
{
    [Column("TopicId")]
    public Guid TopicId { get; set; }

    public Guid? FolderId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [MaxLength(50)]
    public string? FileType { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Unread;

    public int TimeSpentMinutes { get; set; }

    public DateTime? LastOpenedAt { get; set; }

    [ForeignKey(nameof(TopicId))]
    public Topic Topic { get; set; } = null!;

    [ForeignKey(nameof(FolderId))]
    public TopicFolder? Folder { get; set; }

    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
