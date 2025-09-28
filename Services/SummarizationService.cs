using Microsoft.EntityFrameworkCore;
using ChatApi.Data;
using ChatApi.Models;
using Newtonsoft.Json;

namespace ChatApi.Services;

public class SummarizationService : ISummarizationService
{
    private readonly ChatDbContext _context;
    private readonly ILogger<SummarizationService> _logger;

    public SummarizationService(ChatDbContext context, ILogger<SummarizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ConversationSummary> GenerateSummaryAsync(int conversationId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .Include(c => c.KeyPoints)
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {conversationId} not found");
        }

        // Check if summary already exists
        var existingSummary = await _context.Summaries
            .FirstOrDefaultAsync(s => s.ConversationId == conversationId);

        var shortSummary = GenerateShortSummary(conversation);
        var detailedSummary = GenerateDetailedSummary(conversation);
        var mainTopics = ExtractMainTopics(conversation);

        if (existingSummary != null)
        {
            // Update existing summary
            existingSummary.ShortSummary = shortSummary;
            existingSummary.DetailedSummary = detailedSummary;
            existingSummary.MainTopics = JsonConvert.SerializeObject(mainTopics);
            existingSummary.GeneratedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated summary for conversation {ConversationId}", conversationId);
            return existingSummary;
        }
        else
        {
            // Create new summary
            var summary = new ConversationSummary
            {
                ConversationId = conversationId,
                ShortSummary = shortSummary,
                DetailedSummary = detailedSummary,
                MainTopics = JsonConvert.SerializeObject(mainTopics),
                GeneratedAt = DateTime.UtcNow
            };

            _context.Summaries.Add(summary);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated summary for conversation {ConversationId}", conversationId);
            return summary;
        }
    }

    public async Task<ConversationSummary?> GetSummaryAsync(int conversationId)
    {
        return await _context.Summaries
            .FirstOrDefaultAsync(s => s.ConversationId == conversationId);
    }

    public async Task<ConversationSummary> UpdateSummaryAsync(int conversationId)
    {
        return await GenerateSummaryAsync(conversationId);
    }

    public async Task<bool> DeleteSummaryAsync(int conversationId)
    {
        var summary = await _context.Summaries
            .FirstOrDefaultAsync(s => s.ConversationId == conversationId);

        if (summary == null)
        {
            return false;
        }

        _context.Summaries.Remove(summary);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted summary for conversation {ConversationId}", conversationId);
        return true;
    }

    private static string GenerateShortSummary(ChatConversation conversation)
    {
        var messageCount = conversation.Messages.Count;
        var keyPointCount = conversation.KeyPoints.Count;
        var duration = conversation.UpdatedAt - conversation.CreatedAt;

        var participants = conversation.Messages
            .Select(m => m.Role)
            .Distinct()
            .Where(role => role != "system")
            .ToList();

        return $"Conversation with {messageCount} messages over {duration.Days} days, {keyPointCount} key points identified. Participants: {string.Join(", ", participants)}.";
    }

    private static string GenerateDetailedSummary(ChatConversation conversation)
    {
        var summary = new System.Text.StringBuilder();
        
        summary.AppendLine($"**Conversation Title:** {conversation.Title}");
        summary.AppendLine($"**Duration:** {conversation.CreatedAt:yyyy-MM-dd} to {conversation.UpdatedAt:yyyy-MM-dd}");
        summary.AppendLine($"**Total Messages:** {conversation.Messages.Count}");
        summary.AppendLine();

        // Summarize key points by category
        var keyPointsByCategory = conversation.KeyPoints
            .GroupBy(kp => kp.Category ?? "General")
            .ToList();

        if (keyPointsByCategory.Any())
        {
            summary.AppendLine("**Key Points:**");
            foreach (var category in keyPointsByCategory)
            {
                summary.AppendLine($"*{category.Key}:*");
                foreach (var keyPoint in category.OrderByDescending(kp => kp.Priority))
                {
                    var priorityText = keyPoint.Priority switch
                    {
                        3 => "[HIGH]",
                        2 => "[MED]",
                        _ => "[LOW]"
                    };
                    summary.AppendLine($"  - {priorityText} {keyPoint.Content}");
                }
                summary.AppendLine();
            }
        }

        // Add important messages
        var importantMessages = conversation.Messages
            .Where(m => m.IsImportant)
            .Take(3)
            .ToList();

        if (importantMessages.Any())
        {
            summary.AppendLine("**Important Messages:**");
            foreach (var message in importantMessages)
            {
                var preview = message.Content.Length > 100 
                    ? message.Content.Substring(0, 100) + "..."
                    : message.Content;
                summary.AppendLine($"- [{message.Role}] {preview}");
            }
        }

        return summary.ToString();
    }

    private static List<string> ExtractMainTopics(ChatConversation conversation)
    {
        var topics = new List<string>();
        
        // Extract topics from key points categories
        var categories = conversation.KeyPoints
            .Where(kp => !string.IsNullOrWhiteSpace(kp.Category))
            .Select(kp => kp.Category!)
            .Distinct()
            .ToList();

        topics.AddRange(categories);

        // Simple keyword extraction from messages
        var commonWords = new Dictionary<string, int>();
        var stopWords = new HashSet<string> { "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were", "be", "been", "being", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should", "can", "cant", "cannot", "this", "that", "these", "those", "a", "an", "it", "they", "we", "you", "i", "me", "my", "your", "our", "their" };

        foreach (var message in conversation.Messages)
        {
            var words = message.Content.ToLowerInvariant()
                .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 3 && !stopWords.Contains(word))
                .ToList();

            foreach (var word in words)
            {
                commonWords[word] = commonWords.GetValueOrDefault(word, 0) + 1;
            }
        }

        // Add most frequent words as topics
        var topWords = commonWords
            .Where(kvp => kvp.Value >= 2) // Word appears at least twice
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => kvp.Key)
            .ToList();

        topics.AddRange(topWords);

        return topics.Distinct().Take(10).ToList();
    }
}