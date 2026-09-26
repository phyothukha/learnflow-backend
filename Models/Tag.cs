using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace learnflow_service.Models;

[Table("Tags")]
public class Tag : BaseModel
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    public ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
}
