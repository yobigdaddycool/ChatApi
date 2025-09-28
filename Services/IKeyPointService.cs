using ChatApi.Models;

namespace ChatApi.Services;

public interface IKeyPointService
{
    Task<KeyPoint> AddKeyPointAsync(int conversationId, string content, string? category = null, int priority = 1);
    Task<IEnumerable<KeyPoint>> ExtractKeyPointsFromConversationAsync(int conversationId);
    Task<IEnumerable<KeyPoint>> GetKeyPointsAsync(int conversationId);
    Task<bool> DeleteKeyPointAsync(int keyPointId);
    Task<KeyPoint> UpdateKeyPointAsync(int keyPointId, string content, string? category = null, int priority = 1);
}