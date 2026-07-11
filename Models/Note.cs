using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("Notes")]
public class Note : BaseModel
{
    [Column("TopicId")]
    public Guid TopicId { get; set; }

    public Guid? DocumentId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    [ForeignKey(nameof(TopicId))]
    public Topic Topic { get; set; } = null!;

    [ForeignKey(nameof(DocumentId))]
    public Document? Document { get; set; }
}
