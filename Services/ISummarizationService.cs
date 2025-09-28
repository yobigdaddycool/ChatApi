using ChatApi.Models;

namespace ChatApi.Services;

public interface ISummarizationService
{
    Task<ConversationSummary> GenerateSummaryAsync(int conversationId);
    Task<ConversationSummary?> GetSummaryAsync(int conversationId);
    Task<ConversationSummary> UpdateSummaryAsync(int conversationId);
    Task<bool> DeleteSummaryAsync(int conversationId);
}