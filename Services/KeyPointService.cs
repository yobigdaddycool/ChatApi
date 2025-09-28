using Microsoft.EntityFrameworkCore;
using ChatApi.Data;
using ChatApi.Models;

namespace ChatApi.Services;

public class KeyPointService : IKeyPointService
{
    private readonly ChatDbContext _context;
    private readonly ILogger<KeyPointService> _logger;

    public KeyPointService(ChatDbContext context, ILogger<KeyPointService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<KeyPoint> AddKeyPointAsync(int conversationId, string content, string? category = null, int priority = 1)
    {
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {conversationId} not found");
        }

        var keyPoint = new KeyPoint
        {
            ConversationId = conversationId,
            Content = content,
            Category = category,
            Priority = Math.Clamp(priority, 1, 3), // Ensure priority is between 1-3
            CreatedAt = DateTime.UtcNow
        };

        _context.KeyPoints.Add(keyPoint);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added key point to conversation {ConversationId}", conversationId);
        return keyPoint;
    }

    public async Task<IEnumerable<KeyPoint>> ExtractKeyPointsFromConversationAsync(int conversationId)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        var extractedKeyPoints = new List<KeyPoint>();

        // Simple keyword-based extraction logic
        var importantKeywords = new[]
        {
            "important", "key", "remember", "note", "critical", "essential",
            "must", "should", "don't forget", "keep in mind", "takeaway",
            "action item", "todo", "task", "decision", "conclusion"
        };

        foreach (var message in messages)
        {
            var sentences = message.Content.Split('.', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var sentence in sentences)
            {
                var lowerSentence = sentence.Trim().ToLowerInvariant();
                
                // Check if sentence contains important keywords
                if (importantKeywords.Any(keyword => lowerSentence.Contains(keyword)))
                {
                    var keyPoint = new KeyPoint
                    {
                        ConversationId = conversationId,
                        Content = sentence.Trim(),
                        Category = DetermineCategory(sentence.Trim()),
                        Priority = DeterminePriority(lowerSentence, importantKeywords),
                        CreatedAt = DateTime.UtcNow
                    };

                    // Check if similar key point already exists
                    var existingKeyPoint = await _context.KeyPoints
                        .FirstOrDefaultAsync(kp => kp.ConversationId == conversationId && 
                                           kp.Content.ToLower().Contains(lowerSentence.Substring(0, Math.Min(20, lowerSentence.Length))));

                    if (existingKeyPoint == null)
                    {
                        _context.KeyPoints.Add(keyPoint);
                        extractedKeyPoints.Add(keyPoint);
                    }
                }
            }
        }

        if (extractedKeyPoints.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Extracted {Count} key points from conversation {ConversationId}", 
                extractedKeyPoints.Count, conversationId);
        }

        return extractedKeyPoints;
    }

    public async Task<IEnumerable<KeyPoint>> GetKeyPointsAsync(int conversationId)
    {
        return await _context.KeyPoints
            .Where(kp => kp.ConversationId == conversationId)
            .OrderByDescending(kp => kp.Priority)
            .ThenByDescending(kp => kp.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteKeyPointAsync(int keyPointId)
    {
        var keyPoint = await _context.KeyPoints.FindAsync(keyPointId);
        if (keyPoint == null)
        {
            return false;
        }

        _context.KeyPoints.Remove(keyPoint);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted key point with ID: {KeyPointId}", keyPointId);
        return true;
    }

    public async Task<KeyPoint> UpdateKeyPointAsync(int keyPointId, string content, string? category = null, int priority = 1)
    {
        var keyPoint = await _context.KeyPoints.FindAsync(keyPointId);
        if (keyPoint == null)
        {
            throw new ArgumentException($"Key point with ID {keyPointId} not found");
        }

        keyPoint.Content = content;
        keyPoint.Category = category;
        keyPoint.Priority = Math.Clamp(priority, 1, 3);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated key point with ID: {KeyPointId}", keyPointId);
        return keyPoint;
    }

    private static string DetermineCategory(string content)
    {
        var lowerContent = content.ToLowerInvariant();
        
        if (lowerContent.Contains("task") || lowerContent.Contains("todo") || lowerContent.Contains("action"))
            return "Action Item";
        if (lowerContent.Contains("decision") || lowerContent.Contains("decided"))
            return "Decision";
        if (lowerContent.Contains("question") || lowerContent.Contains("unclear"))
            return "Question";
        if (lowerContent.Contains("idea") || lowerContent.Contains("suggestion"))
            return "Idea";
        
        return "General";
    }

    private static int DeterminePriority(string content, string[] keywords)
    {
        var highPriorityKeywords = new[] { "critical", "essential", "must", "urgent" };
        var mediumPriorityKeywords = new[] { "important", "should", "key" };

        if (highPriorityKeywords.Any(keyword => content.Contains(keyword)))
            return 3;
        if (mediumPriorityKeywords.Any(keyword => content.Contains(keyword)))
            return 2;
        
        return 1;
    }
}