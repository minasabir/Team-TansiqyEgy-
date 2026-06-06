using Microsoft.AspNetCore.Mvc;
using TansiqyV1.BLL.ModelVM;
using TansiqyV1.BLL.Services.Abstraction;

namespace TansiqyV1.API.Controllers;

[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(IChatbotService chatbotService, ILogger<ChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _logger = logger;
    }

    [HttpPost("message")]
    [ProducesResponseType(typeof(ChatbotMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ChatbotMessageResponseDto>> SendMessage(
        [FromBody] ChatbotMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _chatbotService.SendMessageAsync(request, cancellationToken);
            if (string.IsNullOrWhiteSpace(result.Response))
            {
                result.Response = "معلش حصل مشكلة، ممكن تحاول تاني؟";
            }

            return Ok(result);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Chatbot request timed out");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Chatbot request timed out" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Chatbot configuration error");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Chatbot upstream error");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Chatbot service is unavailable" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chatbot error");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }
}
