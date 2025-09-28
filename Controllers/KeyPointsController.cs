using Microsoft.AspNetCore.Mvc;
using ChatApi.Services;

namespace ChatApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KeyPointsController : ControllerBase
{
    private readonly IKeyPointService _keyPointService;
    private readonly ILogger<KeyPointsController> _logger;

    public KeyPointsController(IKeyPointService keyPointService, ILogger<KeyPointsController> logger)
    {
        _keyPointService = keyPointService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddKeyPoint([FromBody] AddKeyPointRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Content is required");
            }

            var keyPoint = await _keyPointService.AddKeyPointAsync(
                request.ConversationId, 
                request.Content, 
                request.Category, 
                request.Priority);

            return CreatedAtAction(nameof(GetKeyPoint), new { id = keyPoint.Id }, keyPoint);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding key point");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetKeyPoint(int id)
    {
        // This is a placeholder - we would need to implement GetKeyPointAsync in the service
        return NotFound("Key point retrieval by ID not implemented");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateKeyPoint(int id, [FromBody] UpdateKeyPointRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Content is required");
            }

            var keyPoint = await _keyPointService.UpdateKeyPointAsync(id, request.Content, request.Category, request.Priority);
            return Ok(keyPoint);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating key point {KeyPointId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteKeyPoint(int id)
    {
        try
        {
            var deleted = await _keyPointService.DeleteKeyPointAsync(id);
            if (!deleted)
            {
                return NotFound($"Key point with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting key point {KeyPointId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

public record AddKeyPointRequest(int ConversationId, string Content, string? Category = null, int Priority = 1);
public record UpdateKeyPointRequest(string Content, string? Category = null, int Priority = 1);