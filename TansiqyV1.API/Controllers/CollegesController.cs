using Microsoft.AspNetCore.Mvc;
using TansiqyV1.BLL.Services.Abstraction;

namespace TansiqyV1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollegesController : ControllerBase
{
    private readonly IUniversityService _universityService;
    private readonly ILogger<CollegesController> _logger;

    public CollegesController(
        IUniversityService universityService,
        ILogger<CollegesController> logger)
    {
        _universityService = universityService;
        _logger = logger;
    }

    /// <summary>
    /// Get college by ID
    /// </summary>
    /// <param name="id">College ID</param>
    /// <returns>College details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.CollegeViewModel>> GetById(int id)
    {
        try
        {
            var college = await _universityService.GetCollegeByIdAsync(id);
            
            if (college == null)
            {
                return NotFound(new { message = $"College with ID {id} not found" });
            }

            return Ok(college);
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "Database error getting college by ID {Id}", id);
            return StatusCode(503, new { message = "Database service is temporarily unavailable. Please try again later.", error = "Service Unavailable" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting college by ID {Id}. Error: {Error}", id, ex.Message);
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }
}
