using System.ComponentModel.DataAnnotations;

namespace learnflow_service.Dtos;

public class CourseResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCourseRequest
{
    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public bool IsPublished { get; set; }
}

public class UpdateCourseRequest
{
    [MaxLength(255)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public bool? IsPublished { get; set; }
}

public class CourseQueryParameters
{
    public string? Category { get; set; }
    public bool? IsPublished { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
