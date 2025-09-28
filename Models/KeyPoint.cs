using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Models;

public class KeyPoint
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ConversationId { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string? Category { get; set; }
    
    public int Priority { get; set; } = 1; // 1 = low, 2 = medium, 3 = high
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    [ForeignKey("ConversationId")]
    public ChatConversation Conversation { get; set; } = null!;
}