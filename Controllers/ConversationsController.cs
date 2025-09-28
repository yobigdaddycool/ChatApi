using Microsoft.AspNetCore.Mvc;
using ChatApi.Models;
using ChatApi.Services;
using ChatApi.DTOs;

namespace ChatApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IKeyPointService _keyPointService;
    private readonly ISummarizationService _summarizationService;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(
        IChatService chatService,
        IKeyPointService keyPointService,
        ISummarizationService summarizationService,
        ILogger<ConversationsController> logger)
    {
        _chatService = chatService;
        _keyPointService = keyPointService;
        _summarizationService = summarizationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        try
        {
            var conversations = await _chatService.GetConversationsAsync();
            var conversationDtos = conversations.Select(c => new ConversationSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MessageCount = c.Messages?.Count ?? 0,
                KeyPointCount = c.KeyPoints?.Count ?? 0
            }).ToList();
            
            return Ok(conversationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConversation(int id)
    {
        try
        {
            var conversation = await _chatService.GetConversationAsync(id);
            if (conversation == null)
            {
                return NotFound($"Conversation with ID {id} not found");
            }

            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Description = conversation.Description,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                Messages = conversation.Messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    IsImportant = m.IsImportant
                }).ToList(),
                KeyPoints = conversation.KeyPoints.Select(kp => new KeyPointDto
                {
                    Id = kp.Id,
                    ConversationId = kp.ConversationId,
                    Content = kp.Content,
                    Category = kp.Category,
                    Priority = kp.Priority,
                    CreatedAt = kp.CreatedAt
                }).ToList()
            };

            return Ok(conversationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title is required");
            }

            var conversation = await _chatService.CreateConversationAsync(request.Title, request.Description);
            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Description = conversation.Description,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                Messages = new List<MessageDto>(),
                KeyPoints = new List<KeyPointDto>()
            };
            
            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConversation(int id, [FromBody] UpdateConversationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title is required");
            }

            var conversation = await _chatService.UpdateConversationAsync(id, request.Title, request.Description);
            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Description = conversation.Description,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                Messages = new List<MessageDto>(),
                KeyPoints = new List<KeyPointDto>()
            };
            
            return Ok(conversationDto);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConversation(int id)
    {
        try
        {
            var deleted = await _chatService.DeleteConversationAsync(id);
            if (!deleted)
            {
                return NotFound($"Conversation with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/messages")]
    public async Task<IActionResult> AddMessage(int id, [FromBody] AddMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Role) || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Role and Content are required");
            }

            var message = await _chatService.AddMessageAsync(id, request.Role, request.Content, request.IsImportant);
            
            // Auto-extract key points if this is an important message or from assistant
            if (request.IsImportant || request.Role.ToLowerInvariant() == "assistant")
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _keyPointService.ExtractKeyPointsFromConversationAsync(id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to auto-extract key points for conversation {ConversationId}", id);
                    }
                });
            }

            var messageDto = new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                Role = message.Role,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsImportant = message.IsImportant
            };

            return CreatedAtAction(nameof(GetConversation), new { id }, messageDto);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message to conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        try
        {
            var messages = await _chatService.GetMessagesAsync(id);
            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                Role = m.Role,
                Content = m.Content,
                Timestamp = m.Timestamp,
                IsImportant = m.IsImportant
            }).ToList();
            
            return Ok(messageDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages for conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/keypoints")]
    public async Task<IActionResult> GetKeyPoints(int id)
    {
        try
        {
            var keyPoints = await _keyPointService.GetKeyPointsAsync(id);
            var keyPointDtos = keyPoints.Select(kp => new KeyPointDto
            {
                Id = kp.Id,
                ConversationId = kp.ConversationId,
                Content = kp.Content,
                Category = kp.Category,
                Priority = kp.Priority,
                CreatedAt = kp.CreatedAt
            }).ToList();
            
            return Ok(keyPointDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving key points for conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/keypoints/extract")]
    public async Task<IActionResult> ExtractKeyPoints(int id)
    {
        try
        {
            var keyPoints = await _keyPointService.ExtractKeyPointsFromConversationAsync(id);
            var keyPointDtos = keyPoints.Select(kp => new KeyPointDto
            {
                Id = kp.Id,
                ConversationId = kp.ConversationId,
                Content = kp.Content,
                Category = kp.Category,
                Priority = kp.Priority,
                CreatedAt = kp.CreatedAt
            }).ToList();
            
            return Ok(keyPointDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting key points for conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/summary")]
    public async Task<IActionResult> GetSummary(int id)
    {
        try
        {
            var summary = await _summarizationService.GetSummaryAsync(id);
            if (summary == null)
            {
                // Generate summary if it doesn't exist
                summary = await _summarizationService.GenerateSummaryAsync(id);
            }
            return Ok(summary);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving summary for conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/summary")]
    public async Task<IActionResult> GenerateSummary(int id)
    {
        try
        {
            var summary = await _summarizationService.GenerateSummaryAsync(id);
            return Ok(summary);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary for conversation {ConversationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

public record CreateConversationRequest(string Title, string? Description = null);
public record UpdateConversationRequest(string Title, string? Description = null);
public record AddMessageRequest(string Role, string Content, bool IsImportant = false);