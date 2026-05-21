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

    // ==========================================
    // UNIVERSITY CRUD OPERATIONS
    // ==========================================

    // READ - Universities
    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BLL.ModelVM.UniversityViewModel>>> GetAll()
    {
        try
        {
            var universities = await _universityService.SearchUniversitiesIntelligentAsync(
                null, null, null, null, null, null, null, null, null);
            return Ok(universities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all universities");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    [HttpPut]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BLL.ModelVM.UniversityViewModel>>> GetAllPut()
    {
        try
        {
            var universities = await _universityService.SearchUniversitiesIntelligentAsync(
                null, null, null, null, null, null, null, null, null);
            return Ok(universities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all universities");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

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

    // CREATE - Universities
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BLL.ModelVM.UniversityViewModel>> CreateUniversity([FromForm] BLL.ModelVM.CreateUniversityDto dto)
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating university");
            return StatusCode(500, new { message = "An error occurred while creating the university", error = ex.Message });
        }
    }

    // UPDATE - Universities
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.UniversityViewModel>> UpdateUniversity(
        int id,
        [FromForm] BLL.ModelVM.UpdateUniversityDto dto)
    {
        try
        {
            // Override the Id from route
            dto.Id = id;
            
            if (!Enum.IsDefined(typeof(UniversityType), dto.Type))
            {
                return BadRequest(new { message = "Invalid university type" });
            }

            if (!Enum.IsDefined(typeof(Governorate), dto.Governorate))
            {
                return BadRequest(new { message = "Invalid governorate" });
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

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.UniversityViewModel>> PatchUniversity(
        int id,
        [FromForm] BLL.ModelVM.PatchUniversityDto dto)
    {
        try
        {
            // Get existing university to fill in missing values
            var existing = await _universityService.GetUniversityByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = $"University with ID {id} not found" });
            }

            // Validate enum values if provided
            if (dto.Type.HasValue && !Enum.IsDefined(typeof(UniversityType), dto.Type.Value))
            {
                return BadRequest(new { message = "Invalid university type" });
            }

            if (dto.Governorate.HasValue && !Enum.IsDefined(typeof(Governorate), dto.Governorate.Value))
            {
                return BadRequest(new { message = "Invalid governorate" });
            }

            // Build full DTO from existing + provided values
            // Use existing values when dto values are null or empty (form sends empty strings for missing fields)
            var updateDto = new BLL.ModelVM.UpdateUniversityDto
            {
                Id = id,
                NameAr = string.IsNullOrWhiteSpace(dto.NameAr) ? existing.NameAr : dto.NameAr,
                NameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? existing.NameEn : dto.NameEn,
                Type = dto.Type ?? (UniversityType)existing.Type,
                Governorate = dto.Governorate ?? (Governorate)existing.Governorate,
                OfficialWebsite = string.IsNullOrWhiteSpace(dto.OfficialWebsite) ? existing.OfficialWebsite : dto.OfficialWebsite,
                Location = string.IsNullOrWhiteSpace(dto.Location) ? existing.Location : dto.Location,
                LastYearCoordination = dto.LastYearCoordination ?? existing.LastYearCoordination,
                Fees = dto.Fees ?? existing.Fees,
                InformationSources = string.IsNullOrWhiteSpace(dto.InformationSources) ? existing.InformationSources : dto.InformationSources,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? existing.Description : dto.Description,
                ImageFile = dto.ImageFile,
                RemoveImage = dto.RemoveImage
            };

            var university = await _universityService.UpdateUniversityAsync(updateDto);
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

    // DELETE - Universities
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

    // ==========================================
    // COLLEGE CRUD OPERATIONS
    // ==========================================

    // READ - Colleges
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

    [HttpGet("colleges/{id}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.CollegeViewModel>> GetCollegeById(int id)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting college by ID");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    // CREATE - Colleges
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

    // UPDATE - Colleges
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

    [HttpPatch("colleges/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.CollegeViewModel>> PatchCollege(
        int id,
        [FromBody] BLL.ModelVM.UpdateCollegeDto dto)
    {
        try
        {
            // Ensure ID from route is set
            dto.Id = id;

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

    // DELETE - Colleges
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

    // ==========================================
    // DEPARTMENT CRUD OPERATIONS
    // ==========================================

    // READ - Departments
    [HttpGet("departments/{id}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.DepartmentViewModel>> GetDepartmentById(int id)
    {
        try
        {
            var department = await _universityService.GetDepartmentByIdAsync(id);
            
            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found" });
            }

            return Ok(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department by ID");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    // CREATE - Departments
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

    // UPDATE - Departments
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

    [HttpPatch("departments/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.DepartmentViewModel>> PatchDepartment(
        int id,
        [FromBody] BLL.ModelVM.UpdateDepartmentDto dto)
    {
        try
        {
            // Ensure ID from route is set
            dto.Id = id;

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

    // DELETE - Departments
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

    // ==========================================
    // BRANCH CRUD OPERATIONS
    // ==========================================

    // CREATE - Branches
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

    // UPDATE - Branches
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

    [HttpPatch("{universityId}/branches/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BLL.ModelVM.BranchViewModel>> PatchBranch(
        int universityId,
        int id,
        [FromBody] BLL.ModelVM.UpdateBranchDto branchDto)
    {
        try
        {
            // Get existing branch to fill in missing values
            var existing = await _universityService.GetBranchByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Branch with ID {id} not found" });
            }

            var branch = await _universityService.UpdateBranchAsync(universityId, branchDto);
            return Ok(branch);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    // DELETE - Branches
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

    // ==========================================
    // SEARCH OPERATIONS
    // ==========================================

    [HttpGet("search/intelligent")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "searchTerm", "type", "governorate", "studyType", "minFees", "maxFees", "minCoordination", "maxCoordination", "collegeName" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SearchIntelligent(
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

            // If collegeName is provided, search for colleges
            if (!string.IsNullOrWhiteSpace(collegeName))
            {
                var colleges = await _universityService.SearchCollegesIntelligentAsync(
                    searchTerm,
                    universityType,
                    governorateEnum,
                    studyTypeEnum,
                    minFees,
                    maxFees,
                    minCoordination,
                    maxCoordination,
                    collegeName);

                return Ok(colleges);
            }
            // Otherwise, search for universities
            else
            {
                var universities = await _universityService.SearchUniversitiesIntelligentAsync(
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing intelligent Arabic search");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    [HttpGet("search/name/intelligent")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "searchTerm" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<BLL.ModelVM.SearchResultViewModel>> SearchByNameIntelligent([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Ok(new BLL.ModelVM.SearchResultViewModel
                {
                    Universities = new List<BLL.ModelVM.UniversityViewModel>(),
                    Colleges = new List<BLL.ModelVM.CollegeViewModel>()
                });
            }

            var result = await _universityService.SearchByNameIntelligentAsync(searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching by name with intelligent Arabic matching");
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }
}
