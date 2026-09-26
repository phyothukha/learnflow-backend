using System.ComponentModel.DataAnnotations;

namespace learnflow_service.Dtos;

public class TopicResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTopicRequest
{
    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }
}

public class UpdateTopicRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public bool? IsArchived { get; set; }
}

public class TopicQueryParameters
{
    public bool? IsArchived { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
