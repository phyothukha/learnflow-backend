using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace learnflow_service.Models;

[Table("DocumentTags")]
public class DocumentTag
{
    public Guid DocumentId { get; set; }
    public Guid TagId { get; set; }

    [ForeignKey(nameof(DocumentId)), ValidateNever]
    public Document Document { get; set; } = null!;

    [ForeignKey(nameof(TagId)), ValidateNever]
    public Tag Tag { get; set; } = null!;
}
