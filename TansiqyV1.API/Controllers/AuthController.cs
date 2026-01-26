using Microsoft.AspNetCore.Mvc;
using TansiqyV1.BLL.ModelVM;
using TansiqyV1.BLL.Services.Abstraction;

namespace TansiqyV1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Admin Login - Authenticate and receive JWT token
    /// </summary>
    /// <param name="request">Login credentials (Email and Password)</param>
    /// <returns>JWT token with user information</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during authentication", error = ex.Message });
        }
    }
}

