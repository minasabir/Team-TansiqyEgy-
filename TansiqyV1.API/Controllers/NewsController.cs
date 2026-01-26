using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TansiqyV1.BLL.Services.Abstraction;

namespace TansiqyV1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;
    private readonly ILogger<NewsController> _logger;

    public NewsController(
        INewsService newsService,
        ILogger<NewsController> logger)
    {
        _newsService = newsService;
        _logger = logger;
    }

    /// <summary>
    /// Get all news (Public)
    /// </summary>
    /// <returns>List of news</returns>
    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BLL.ModelVM.NewsViewModel>>> GetAllNews()
    {
        try
        {
            var news = await _newsService.GetAllNewsAsync();
            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting news");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Get news by ID (Public)
    /// </summary>
    /// <param name="id">News ID</param>
    /// <returns>News details</returns>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.NewsViewModel>> GetNewsById(int id)
    {
        try
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            
            if (news == null)
            {
                return NotFound(new { message = $"News with ID {id} not found" });
            }

            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting news by ID");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new news (Admin only)
    /// </summary>
    /// <param name="dto">News data</param>
    /// <returns>Created news</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BLL.ModelVM.NewsViewModel>> CreateNews([FromBody] BLL.ModelVM.CreateNewsDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var news = await _newsService.CreateNewsAsync(dto);
            return Created($"/api/News/{news.Id}", news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news");
            return StatusCode(500, new { message = "An error occurred while creating the news", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a news (Admin only)
    /// </summary>
    /// <param name="dto">News data</param>
    /// <returns>Updated news</returns>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.NewsViewModel>> UpdateNews([FromBody] BLL.ModelVM.UpdateNewsDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var news = await _newsService.UpdateNewsAsync(dto);
            return Ok(news);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news");
            return StatusCode(500, new { message = "An error occurred while updating the news", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a news (Admin only) - PATCH
    /// </summary>
    /// <param name="id">News ID</param>
    /// <param name="dto">News data</param>
    /// <returns>Updated news</returns>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.NewsViewModel>> PatchNews(int id, [FromBody] BLL.ModelVM.UpdateNewsDto dto)
    {
        try
        {
            dto.Id = id; // Ensure ID matches route
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var news = await _newsService.UpdateNewsAsync(dto);
            return Ok(news);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news");
            return StatusCode(500, new { message = "An error occurred while updating the news", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a news (Admin only)
    /// </summary>
    /// <param name="id">News ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteNews(int id)
    {
        try
        {
            var result = await _newsService.DeleteNewsAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"News with ID {id} not found" });
            }

            return Ok(new { message = "News deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news");
            return StatusCode(500, new { message = "An error occurred while deleting the news", error = ex.Message });
        }
    }
}

