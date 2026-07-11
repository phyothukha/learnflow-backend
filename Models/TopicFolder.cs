using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("TopicFolders")]
public class TopicFolder : BaseModel
{
    [Column("TopicId")]
    public Guid TopicId { get; set; }

    public Guid? ParentFolderId { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(TopicId))]
    public Topic Topic { get; set; } = null!;

    [ForeignKey(nameof(ParentFolderId))]
    public TopicFolder? ParentFolder { get; set; }

    public ICollection<TopicFolder> Children { get; set; } = new List<TopicFolder>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
