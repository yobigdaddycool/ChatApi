using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ChatApi.Data;
using ChatApi.Services;
using ChatApi.Models;

namespace ChatApi.Tests;

public class ChatServiceTests : IDisposable
{
    private readonly ChatDbContext _context;
    private readonly ChatService _chatService;
    private readonly ILogger<ChatService> _logger;

    public ChatServiceTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ChatDbContext(options);
        _logger = new LoggerFactory().CreateLogger<ChatService>();
        _chatService = new ChatService(_context, _logger);
    }

    [Fact]
    public async Task CreateConversationAsync_ShouldCreateConversation()
    {
        // Arrange
        var title = "Test Conversation";
        var description = "Test Description";

        // Act
        var result = await _chatService.CreateConversationAsync(title, description);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(description, result.Description);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task AddMessageAsync_ShouldAddMessage()
    {
        // Arrange
        var conversation = await _chatService.CreateConversationAsync("Test", "Test");
        var role = "user";
        var content = "Test message";

        // Act
        var result = await _chatService.AddMessageAsync(conversation.Id, role, content);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(conversation.Id, result.ConversationId);
        Assert.Equal(role, result.Role);
        Assert.Equal(content, result.Content);
    }

    [Fact]
    public async Task GetConversationAsync_ShouldReturnConversationWithMessages()
    {
        // Arrange
        var conversation = await _chatService.CreateConversationAsync("Test", "Test");
        await _chatService.AddMessageAsync(conversation.Id, "user", "Hello");
        await _chatService.AddMessageAsync(conversation.Id, "assistant", "Hi there");

        // Act
        var result = await _chatService.GetConversationAsync(conversation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(conversation.Id, result.Id);
        Assert.Equal(2, result.Messages.Count);
    }

    [Fact]
    public async Task DeleteConversationAsync_ShouldDeleteConversation()
    {
        // Arrange
        var conversation = await _chatService.CreateConversationAsync("Test", "Test");

        // Act
        var deleted = await _chatService.DeleteConversationAsync(conversation.Id);
        var result = await _chatService.GetConversationAsync(conversation.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}