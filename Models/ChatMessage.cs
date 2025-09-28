using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Models;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ConversationId { get; set; }
    
    [Required]
    public string Role { get; set; } = string.Empty; // "user", "assistant", "system"
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public bool IsImportant { get; set; } = false;
    
    // Navigation property
    [ForeignKey("ConversationId")]
    public ChatConversation Conversation { get; set; } = null!;
}