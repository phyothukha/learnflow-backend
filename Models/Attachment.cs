using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace learnflow_service.Models;

[Table("Attachments")]
public class Attachment : BaseModel
{
    public Guid DocumentId { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = null!;

    [Required, MaxLength(1000)]
    public string StorageUrl { get; set; } = null!;

    [Required, MaxLength(255)]
    public string StoragePath { get; set; } = null!;

    [Required, MaxLength(100)]
    public string ContentType { get; set; } = null!;

    public long SizeBytes { get; set; }

    [ForeignKey(nameof(DocumentId)), ValidateNever]
    public Document Document { get; set; } = null!;
}
