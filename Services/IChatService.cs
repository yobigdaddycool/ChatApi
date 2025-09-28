using ChatApi.Models;

namespace ChatApi.Services;

public interface IChatService
{
    Task<ChatConversation> CreateConversationAsync(string title, string? description = null);
    Task<ChatMessage> AddMessageAsync(int conversationId, string role, string content, bool isImportant = false);
    Task<IEnumerable<ChatConversation>> GetConversationsAsync();
    Task<ChatConversation?> GetConversationAsync(int id);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId);
    Task<bool> DeleteConversationAsync(int id);
    Task<ChatConversation> UpdateConversationAsync(int id, string title, string? description = null);
}