using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TansiqyV1.BLL.Services.Abstraction;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UniversitiesController : ControllerBase
{
    private readonly IUniversityService _universityService;
    private readonly ILogger<UniversitiesController> _logger;

    public UniversitiesController(
        IUniversityService universityService,
        ILogger<UniversitiesController> logger)
    {
        _universityService = universityService;
        _logger = logger;
    }

    /// <summary>
    /// Get all university types with counts
    /// </summary>
    /// <returns>List of university types with Arabic names and counts</returns>
    [HttpGet("types")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BLL.ModelVM.UniversityTypeViewModel>>> GetUniversityTypes()
    {
        try
        {
            var types = await _universityService.GetUniversityTypesAsync();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting university types");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all universities by type
    /// </summary>
    /// <param name="type">University type (1=Governmental, 2=Private, 3=National, 4=HigherInstitute, 5=Foreign, 6=Technological)</param>
    /// <returns>List of universities</returns>
    [HttpGet("type/{type}")]
    [ResponseCache(Duration = 180, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "type" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<BLL.ModelVM.UniversityViewModel>>> GetByType(int type)
    {
        try
        {
            if (!Enum.IsDefined(typeof(UniversityType), type))
            {
                return BadRequest(new { message = "Invalid university type. Valid types: 1=Governmental, 2=Private, 3=National, 4=HigherInstitute, 5=Foreign, 6=Technological" });
            }

            var universities = await _universityService.GetUniversitiesByTypeAsync((UniversityType)type);
            var universitiesList = universities?.ToList() ?? new List<BLL.ModelVM.UniversityViewModel>();
            return Ok(universitiesList);
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "Database error getting universities by type {Type}. Error: {Error}", type, sqlEx.Message);
            return StatusCode(503, new { message = "Database service is temporarily unavailable. Please try again later.", error = "Service Unavailable" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting universities by type {Type}. Error: {Error}", type, ex.Message);
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Get university by ID
    /// </summary>
    /// <param name="id">University ID</param>
    /// <returns>University details</returns>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.UniversityViewModel>> GetById(int id)
    {
        try
        {
            var university = await _universityService.GetUniversityByIdAsync(id);
            
            if (university == null)
            {
                return NotFound(new { message = $"University with ID {id} not found" });
            }

            return Ok(university);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting university by ID");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Search universities by name only (for search bar)
    /// </summary>
    /// <param name="searchTerm">Search term (searches in university name only)</param>
    /// <returns>List of matching universities</returns>
    [HttpGet("search/name")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "searchTerm" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BLL.ModelVM.UniversityViewModel>>> SearchByName([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Ok(new List<BLL.ModelVM.UniversityViewModel>());
            }

            var universities = await _universityService.SearchUniversitiesByNameAsync(searchTerm);
            return Ok(universities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching universities by name");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Search universities with filters
    /// </summary>
    /// <param name="searchTerm">Search term (searches in name only - strict filtering)</param>
    /// <param name="type">University type (1=Governmental, 2=Private, 3=National, 4=HigherInstitute, 5=Foreign, 6=Technological)</param>
    /// <param name="governorate">Governorate ID</param>
    /// <param name="studyType">Study type (1=Math, 2=Science, 3=Literary, 4=Industrial, 5=American)</param>
    /// <param name="minFees">Minimum fees</param>
    /// <param name="maxFees">Maximum fees</param>
    /// <param name="minCoordination">Minimum coordination</param>
    /// <param name="maxCoordination">Maximum coordination</param>
    /// <param name="collegeName">College name (searches in college name)</param>
    /// <returns>List of matching universities</returns>
    [HttpGet("search")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "searchTerm", "type", "governorate", "studyType", "minFees", "maxFees", "minCoordination", "maxCoordination", "collegeName" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BLL.ModelVM.UniversityViewModel>>> Search(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? type = null,
        [FromQuery] int? governorate = null,
        [FromQuery] int? studyType = null,
        [FromQuery] decimal? minFees = null,
        [FromQuery] decimal? maxFees = null,
        [FromQuery] decimal? minCoordination = null,
        [FromQuery] decimal? maxCoordination = null,
        [FromQuery] string? collegeName = null)
    {
        try
        {
            UniversityType? universityType = type.HasValue && Enum.IsDefined(typeof(UniversityType), type.Value)
                ? (UniversityType?)type.Value
                : null;

            Governorate? governorateEnum = governorate.HasValue && Enum.IsDefined(typeof(Governorate), governorate.Value)
                ? (Governorate?)governorate.Value
                : null;

            StudyType? studyTypeEnum = studyType.HasValue && Enum.IsDefined(typeof(StudyType), studyType.Value)
                ? (StudyType?)studyType.Value
                : null;

            var universities = await _universityService.SearchUniversitiesAsync(
                searchTerm,
                universityType,
                governorateEnum,
                studyTypeEnum,
                minFees,
                maxFees,
                minCoordination,
                maxCoordination,
                collegeName);

            return Ok(universities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching universities");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all colleges for a specific university
    /// </summary>
    /// <param name="id">University ID</param>
    /// <returns>List of colleges</returns>
    [HttpGet("{id}/colleges")]
    [ResponseCache(Duration = 180, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<BLL.ModelVM.CollegeViewModel>>> GetColleges(int id)
    {
        try
        {
            // First verify university exists
            var university = await _universityService.GetUniversityByIdAsync(id);
            if (university == null)
            {
                return NotFound(new { message = $"University with ID {id} not found" });
            }

            var colleges = await _universityService.GetCollegesByUniversityIdAsync(id);
            return Ok(colleges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting colleges for university");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new university (Admin only)
    /// </summary>
    /// <param name="dto">University data</param>
    /// <returns>Created university</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BLL.ModelVM.UniversityViewModel>> CreateUniversity([FromBody] BLL.ModelVM.CreateUniversityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var university = await _universityService.CreateUniversityAsync(dto);
            return Created($"/api/Universities/{university.Id}", university);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating university");
            return StatusCode(500, new { message = "An error occurred while creating the university", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new college (Admin only)
    /// </summary>
    /// <param name="dto">College data</param>
    /// <returns>Created college</returns>
    [HttpPost("colleges")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BLL.ModelVM.CollegeViewModel>> CreateCollege([FromBody] BLL.ModelVM.CreateCollegeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var college = await _universityService.CreateCollegeAsync(dto);
            return Created($"/api/Universities/{college.UniversityId}/colleges/{college.Id}", college);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating college");
            return StatusCode(500, new { message = "An error occurred while creating the college", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new department (Admin only)
    /// </summary>
    /// <param name="dto">Department data</param>
    /// <returns>Created department</returns>
    [HttpPost("departments")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BLL.ModelVM.DepartmentViewModel>> CreateDepartment([FromBody] BLL.ModelVM.CreateDepartmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var department = await _universityService.CreateDepartmentAsync(dto);
            return Created($"/api/Colleges/{dto.CollegeId}/departments/{department.Id}", department);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return StatusCode(500, new { message = "An error occurred while creating the department", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new branch for a university (Admin only)
    /// </summary>
    /// <param name="universityId">University ID</param>
    /// <param name="dto">Branch data</param>
    /// <returns>Created branch</returns>
    [HttpPost("{universityId}/branches")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BLL.ModelVM.BranchViewModel>> CreateBranch(int universityId, [FromBody] BLL.ModelVM.CreateBranchDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var branch = await _universityService.CreateBranchAsync(universityId, dto);
            return Created($"/api/Universities/{universityId}/branches/{branch.Id}", branch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch");
            return StatusCode(500, new { message = "An error occurred while creating the branch", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a university (Admin only)
    /// </summary>
    /// <param name="dto">University data</param>
    /// <returns>Updated university</returns>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.UniversityViewModel>> UpdateUniversity([FromBody] BLL.ModelVM.UpdateUniversityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var university = await _universityService.UpdateUniversityAsync(dto);
            return Ok(university);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating university");
            return StatusCode(500, new { message = "An error occurred while updating the university", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a university (Admin only) - PATCH
    /// </summary>
    /// <param name="id">University ID</param>
    /// <param name="dto">University data</param>
    /// <returns>Updated university</returns>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.UniversityViewModel>> PatchUniversity(int id, [FromBody] BLL.ModelVM.UpdateUniversityDto dto)
    {
        try
        {
            dto.Id = id; // Ensure ID matches route
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var university = await _universityService.UpdateUniversityAsync(dto);
            return Ok(university);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating university");
            return StatusCode(500, new { message = "An error occurred while updating the university", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a university (Admin only)
    /// </summary>
    /// <param name="id">University ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUniversity(int id)
    {
        try
        {
            var result = await _universityService.DeleteUniversityAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"University with ID {id} not found" });
            }

            return Ok(new { message = "University deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting university");
            return StatusCode(500, new { message = "An error occurred while deleting the university", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a college (Admin only)
    /// </summary>
    /// <param name="dto">College data</param>
    /// <returns>Updated college</returns>
    [HttpPut("colleges")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.CollegeViewModel>> UpdateCollege([FromBody] BLL.ModelVM.UpdateCollegeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var college = await _universityService.UpdateCollegeAsync(dto);
            return Ok(college);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating college");
            return StatusCode(500, new { message = "An error occurred while updating the college", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a college (Admin only) - PATCH
    /// </summary>
    /// <param name="id">College ID</param>
    /// <param name="dto">College data</param>
    /// <returns>Updated college</returns>
    [HttpPatch("colleges/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.CollegeViewModel>> PatchCollege(int id, [FromBody] BLL.ModelVM.UpdateCollegeDto dto)
    {
        try
        {
            dto.Id = id; // Ensure ID matches route
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var college = await _universityService.UpdateCollegeAsync(dto);
            return Ok(college);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating college");
            return StatusCode(500, new { message = "An error occurred while updating the college", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a college (Admin only)
    /// </summary>
    /// <param name="id">College ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("colleges/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCollege(int id)
    {
        try
        {
            var result = await _universityService.DeleteCollegeAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"College with ID {id} not found" });
            }

            return Ok(new { message = "College deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting college");
            return StatusCode(500, new { message = "An error occurred while deleting the college", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a department (Admin only)
    /// </summary>
    /// <param name="dto">Department data</param>
    /// <returns>Updated department</returns>
    [HttpPut("departments")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.DepartmentViewModel>> UpdateDepartment([FromBody] BLL.ModelVM.UpdateDepartmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var department = await _universityService.UpdateDepartmentAsync(dto);
            return Ok(department);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department");
            return StatusCode(500, new { message = "An error occurred while updating the department", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a department (Admin only) - PATCH
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <param name="dto">Department data</param>
    /// <returns>Updated department</returns>
    [HttpPatch("departments/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.DepartmentViewModel>> PatchDepartment(int id, [FromBody] BLL.ModelVM.UpdateDepartmentDto dto)
    {
        try
        {
            dto.Id = id; // Ensure ID matches route
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var department = await _universityService.UpdateDepartmentAsync(dto);
            return Ok(department);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department");
            return StatusCode(500, new { message = "An error occurred while updating the department", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a department (Admin only)
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("departments/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDepartment(int id)
    {
        try
        {
            var result = await _universityService.DeleteDepartmentAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Department with ID {id} not found" });
            }

            return Ok(new { message = "Department deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department");
            return StatusCode(500, new { message = "An error occurred while deleting the department", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a branch (Admin only)
    /// </summary>
    /// <param name="universityId">University ID</param>
    /// <param name="dto">Branch data</param>
    /// <returns>Updated branch</returns>
    [HttpPut("{universityId}/branches")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.BranchViewModel>> UpdateBranch(int universityId, [FromBody] BLL.ModelVM.UpdateBranchDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var branch = await _universityService.UpdateBranchAsync(universityId, dto);
            return Ok(branch);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch");
            return StatusCode(500, new { message = "An error occurred while updating the branch", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a branch (Admin only) - PATCH
    /// </summary>
    /// <param name="universityId">University ID</param>
    /// <param name="id">Branch ID</param>
    /// <param name="dto">Branch data</param>
    /// <returns>Updated branch</returns>
    [HttpPatch("{universityId}/branches/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.BranchViewModel>> PatchBranch(int universityId, int id, [FromBody] BLL.ModelVM.UpdateBranchDto dto)
    {
        try
        {
            dto.Id = id; // Ensure ID matches route
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var branch = await _universityService.UpdateBranchAsync(universityId, dto);
            return Ok(branch);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch");
            return StatusCode(500, new { message = "An error occurred while updating the branch", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a branch (Admin only)
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("branches/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteBranch(int id)
    {
        try
        {
            var result = await _universityService.DeleteBranchAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Branch with ID {id} not found" });
            }

            return Ok(new { message = "Branch deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch");
            return StatusCode(500, new { message = "An error occurred while deleting the branch", error = ex.Message });
        }
    }
}
