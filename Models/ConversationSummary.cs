using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Models;

public class ConversationSummary
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ConversationId { get; set; }
    
    [Required]
    public string ShortSummary { get; set; } = string.Empty;
    
    public string? DetailedSummary { get; set; }
    
    public string? MainTopics { get; set; } // JSON array of topics
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    [ForeignKey("ConversationId")]
    public ChatConversation Conversation { get; set; } = null!;
}