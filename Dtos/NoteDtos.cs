using System.ComponentModel.DataAnnotations;

namespace learnflow_service.Dtos;

public class NoteResponse
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public Guid? DocumentId { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateNoteRequest
{
    [Required]
    public Guid TopicId { get; set; }

    public Guid? DocumentId { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Content { get; set; }
}

public class UpdateNoteRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }

    public string? Content { get; set; }
    public Guid? DocumentId { get; set; }
}

public class NoteQueryParameters
{
    public Guid? TopicId { get; set; }
    public Guid? DocumentId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
