namespace learnflow_service.Dtos;

public class TagResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int DocumentCount { get; set; }
}
