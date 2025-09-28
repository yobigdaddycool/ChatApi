using Microsoft.EntityFrameworkCore;
using ChatApi.Data;
using ChatApi.Models;

namespace ChatApi.Services;

public class ChatService : IChatService
{
    private readonly ChatDbContext _context;
    private readonly ILogger<ChatService> _logger;

    public ChatService(ChatDbContext context, ILogger<ChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ChatConversation> CreateConversationAsync(string title, string? description = null)
    {
        var conversation = new ChatConversation
        {
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new conversation with ID: {ConversationId}", conversation.Id);
        return conversation;
    }

    public async Task<ChatMessage> AddMessageAsync(int conversationId, string role, string content, bool isImportant = false)
    {
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {conversationId} not found");
        }

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            Role = role,
            Content = content,
            IsImportant = isImportant,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        
        // Update conversation timestamp
        conversation.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added message to conversation {ConversationId}", conversationId);
        return message;
    }

    public async Task<IEnumerable<ChatConversation>> GetConversationsAsync()
    {
        return await _context.Conversations
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<ChatConversation?> GetConversationAsync(int id)
    {
        return await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .Include(c => c.KeyPoints.OrderByDescending(kp => kp.Priority).ThenByDescending(kp => kp.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<bool> DeleteConversationAsync(int id)
    {
        var conversation = await _context.Conversations.FindAsync(id);
        if (conversation == null)
        {
            return false;
        }

        _context.Conversations.Remove(conversation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted conversation with ID: {ConversationId}", id);
        return true;
    }

    public async Task<ChatConversation> UpdateConversationAsync(int id, string title, string? description = null)
    {
        var conversation = await _context.Conversations.FindAsync(id);
        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {id} not found");
        }

        conversation.Title = title;
        conversation.Description = description;
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated conversation with ID: {ConversationId}", id);
        return conversation;
    }
}