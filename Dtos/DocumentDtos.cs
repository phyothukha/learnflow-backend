using System.ComponentModel.DataAnnotations;
using learnflow_service.Models;

namespace learnflow_service.Dtos;

public class AttachmentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DocumentResponse
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public Guid? FolderId { get; set; }
    public string Title { get; set; } = null!;
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public string? Content { get; set; }
    public DocumentStatus Status { get; set; }
    public int TimeSpentMinutes { get; set; }
    public DateTime? LastOpenedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<AttachmentResponse> Attachments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateDocumentRequest
{
    [Required]
    public Guid TopicId { get; set; }

    public Guid? FolderId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [MaxLength(50)]
    public string? FileType { get; set; }

    public string? Content { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Unread;

    public List<string> Tags { get; set; } = new();
}

public class UpdateDocumentRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [MaxLength(50)]
    public string? FileType { get; set; }

    public string? Content { get; set; }

    public DocumentStatus? Status { get; set; }
    public int? TimeSpentMinutes { get; set; }
    public DateTime? LastOpenedAt { get; set; }
    public List<string>? Tags { get; set; }
}

public class MoveDocumentRequest
{
    public Guid? FolderId { get; set; }
}

public class DocumentQueryParameters
{
    public Guid? TopicId { get; set; }
    public Guid? FolderId { get; set; }
    public DocumentStatus? Status { get; set; }
    public string? Tag { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
