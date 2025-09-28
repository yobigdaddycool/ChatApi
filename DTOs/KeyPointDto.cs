namespace ChatApi.DTOs;

public class KeyPointDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
}