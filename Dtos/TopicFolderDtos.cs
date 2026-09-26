using System.ComponentModel.DataAnnotations;

namespace learnflow_service.Dtos;

public class TopicFolderResponse
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public Guid? ParentFolderId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TopicFolderTreeNode
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public Guid? ParentFolderId { get; set; }
    public string Name { get; set; } = null!;
    public List<TopicFolderTreeNode> Children { get; set; } = new();
}

public class CreateTopicFolderRequest
{
    [Required]
    public Guid TopicId { get; set; }

    public Guid? ParentFolderId { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; } = null!;
}

public class UpdateTopicFolderRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }
}

public class MoveTopicFolderRequest
{
    public Guid? ParentFolderId { get; set; }
}

public class TopicFolderQueryParameters
{
    public Guid? TopicId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
