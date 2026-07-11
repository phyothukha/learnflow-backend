using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("Topics")]
public class Topic : BaseModel
{
    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public bool IsArchived { get; set; }

    public ICollection<TopicFolder> Folders { get; set; } = new List<TopicFolder>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<StudyBlock> StudyBlocks { get; set; } = new List<StudyBlock>();
}
