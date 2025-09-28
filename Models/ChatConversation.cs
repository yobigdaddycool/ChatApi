using System.ComponentModel.DataAnnotations;

namespace ChatApi.Models;

public class ChatConversation
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<KeyPoint> KeyPoints { get; set; } = new List<KeyPoint>();
}