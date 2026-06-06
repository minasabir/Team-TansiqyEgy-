# Appendix: Code Documentation - Tansiqy System

This appendix contains important code snippets from the three-layer architecture of the Tansiqy University Information System.

---

## Table of Contents

1. [API Layer](#api-layer)
   - Program.cs Configuration
   - Controllers
2. [BLL Layer](#bll-layer)
   - Service Interfaces
   - Service Implementations
3. [DAL Layer](#dal-layer)
   - Database Context
   - Entities
   - Repositories
   - Enums

---

## API Layer

### Program.cs - Application Configuration

**Description**: The main entry point of the API application. Configures services, authentication, database, CORS, Swagger, and the HTTP request pipeline.

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Repo.Abstraction;
using TansiqyV1.DAL.Repo.Implementation;
using TansiqyV1.BLL.Services.Abstraction;
using TansiqyV1.BLL.Services.Implementation;
using TansiqyV1.API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as integers (not English names) - Arabic strings are provided separately
        // Ignore null values
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Add Response Caching
builder.Services.AddResponseCaching();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var issuer = jwtSettings["Issuer"] ?? "TansiqyAPI";
var audience = jwtSettings["Audience"] ?? "TansiqyClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Remove delay of token when expire
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    // Keep Cookie authentication for PL (frontend)
    options.Cookie.Name = ".AspNetCore.Cookies";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization(options =>
{
    // Configure authorization to return 401 instead of 404 for unauthorized requests
    options.FallbackPolicy = null;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tansiqy API",
        Version = "v1",
        Description = "API for Tansiqy - University and College Information System",
        Contact = new OpenApiContact
        {
            Name = "Tansiqy API Support"
        }
    });
    
    // Add JWT Bearer security definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    // Add security requirement for Bearer scheme
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Include XML comments for better Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS Configuration - Allow all origins for API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=db35733.public.databaseasp.net; Database=db35733; User Id=db35733; Password=Pb4%a7?YM!k6; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUniversityRepository, UniversityRepository>();
builder.Services.AddScoped<ICollegeRepository, CollegeRepository>();
builder.Services.AddScoped<TansiqyV1.DAL.Repo.Abstraction.INewsRepository, TansiqyV1.DAL.Repo.Implementation.NewsRepository>();

// Register Services
builder.Services.AddScoped<IUniversityService>(sp =>
{
    try
    {
        var universityRepo = sp.GetRequiredService<IUniversityRepository>();
        var collegeRepo = sp.GetRequiredService<ICollegeRepository>();
        var departmentRepo = sp.GetRequiredService<IGenericRepository<TansiqyV1.DAL.Entities.Department>>();
        var branchRepo = sp.GetRequiredService<IGenericRepository<TansiqyV1.DAL.Entities.UniversityBranch>>();
        var env = sp.GetRequiredService<IWebHostEnvironment>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<UniversityService>();
        
        var webRootPath = !string.IsNullOrEmpty(env.WebRootPath) 
            ? env.WebRootPath 
            : Path.Combine(env.ContentRootPath, "wwwroot");
        var uploadsPath = Path.Combine(webRootPath, "uploads", "universities");
        
        logger.LogInformation("UniversityService initializing with uploads path: {UploadsPath}", uploadsPath);
        
        return new UniversityService(universityRepo, collegeRepo, departmentRepo, branchRepo, uploadsPath);
    }
    catch (Exception ex)
    {
        var loggerFactory = sp.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger<Program>() ?? 
                     sp.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to initialize UniversityService");
        throw;
    }
});
builder.Services.AddScoped<TansiqyV1.BLL.Services.Abstraction.INewsService, TansiqyV1.BLL.Services.Implementation.NewsService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHttpClient("Chatbot", (sp, client) =>
{
    var timeoutSeconds = sp.GetRequiredService<IConfiguration>().GetValue("Chatbot:TimeoutSeconds", 120);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddScoped<IChatbotService, ChatbotService>();

var app = builder.Build();

// Apply database migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // Test database connectivity first
            if (await context.Database.CanConnectAsync())
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogError("Cannot connect to database. Migrations skipped.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            // Don't throw - allow app to start even if migrations fail
        }
    }
}
catch (Exception ex)
{
    // Log to console as fallback if service provider fails
    Console.WriteLine($"Critical error during migration setup: {ex.Message}");
}

// Configure the HTTP request pipeline.
// Enable Swagger in all environments (or use IsDevelopment() for production safety)
app.MapOpenApi();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tansiqy API V1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
});

// Enable CORS - Must be before UseAuthorization
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Serve static files from wwwroot (for uploaded images)
app.UseStaticFiles();

// Use Response Caching
app.UseResponseCaching();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### UniversitiesController - Main API Controller

**Description**: Handles all HTTP requests for universities, colleges, departments, and branches. Implements CRUD operations and intelligent search functionality.

```csharp
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
    // SEARCH OPERATIONS
    // ==========================================

    [HttpGet("search/intelligent")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "searchTerm", "type", "governorate", "studyType", "minFees", "maxFees", "minCoordination", "maxCoordination", "collegeName" })]
    [ProducesResponseType(typeof(IEnumerable<BLL.ModelVM.UniversityViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<BLL.ModelVM.CollegeViewModel>), StatusCodes.Status200OK)]
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

            var usesCollegeFilters = studyTypeEnum.HasValue
                || minFees.HasValue
                || maxFees.HasValue
                || minCoordination.HasValue
                || maxCoordination.HasValue
                || !string.IsNullOrWhiteSpace(collegeName);

            if (!usesCollegeFilters)
            {
                var universities = await _universityService.SearchUniversitiesIntelligentAsync(
                    searchTerm,
                    universityType,
                    governorateEnum,
                    studyType: null,
                    minFees: null,
                    maxFees: null,
                    minCoordination: null,
                    maxCoordination: null,
                    collegeName: null);

                return Ok(universities);
            }

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
```

### CollegesController - College API Controller

**Description**: Handles HTTP requests specifically for college operations.

```csharp
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
```

---

## BLL Layer

### IUniversityService - Service Interface

**Description**: Defines the contract for university-related business logic operations including CRUD, search, and intelligent Arabic search.

```csharp
using TansiqyV1.BLL.ModelVM;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.Services.Abstraction;

public interface IUniversityService
{
    // Get Methods
    Task<IEnumerable<UniversityTypeViewModel>> GetUniversityTypesAsync();
    Task<IEnumerable<UniversityViewModel>> GetUniversitiesByTypeAsync(UniversityType type);
    Task<UniversityViewModel?> GetUniversityByIdAsync(int id);
    Task<IEnumerable<UniversityViewModel>> SearchUniversitiesAsync(
        string? searchTerm,
        UniversityType? type,
        Governorate? governorate,
        StudyType? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName = null
    );
    Task<IEnumerable<UniversityViewModel>> SearchUniversitiesByNameAsync(string searchTerm);
    
    // Intelligent Arabic Search Methods (new)
    Task<IEnumerable<UniversityViewModel>> SearchUniversitiesIntelligentAsync(
        string? searchTerm,
        UniversityType? type,
        Governorate? governorate,
        StudyType? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName = null
    );

    // Combined search by name only - returns both universities and colleges
    Task<SearchResultViewModel> SearchByNameIntelligentAsync(string searchTerm);

    // Search colleges with filters - returns colleges directly
    Task<IEnumerable<CollegeViewModel>> SearchCollegesIntelligentAsync(
        string? searchTerm,
        UniversityType? type,
        Governorate? governorate,
        StudyType? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName = null
    );
    
    Task<IEnumerable<CollegeViewModel>> GetCollegesByUniversityIdAsync(int universityId);
    Task<CollegeViewModel?> GetCollegeByIdAsync(int collegeId);
    Task<SimpleDepartmentViewModel?> GetDepartmentByIdAsync(int id);
    Task<BranchViewModel?> GetBranchByIdAsync(int id);

    // Create Methods
    Task<UniversityViewModel> CreateUniversityAsync(CreateUniversityDto dto);
    Task<CollegeViewModel> CreateCollegeAsync(CreateCollegeDto dto);
    Task<DepartmentViewModel> CreateDepartmentAsync(CreateDepartmentDto dto);
    Task<BranchViewModel> CreateBranchAsync(int universityId, CreateBranchDto dto);

    // Update Methods
    Task<UniversityViewModel> UpdateUniversityAsync(UpdateUniversityDto dto);
    Task<CollegeViewModel> UpdateCollegeAsync(UpdateCollegeDto dto);
    Task<DepartmentViewModel> UpdateDepartmentAsync(UpdateDepartmentDto dto);
    Task<BranchViewModel> UpdateBranchAsync(int universityId, UpdateBranchDto dto);

    // Delete Methods
    Task<bool> DeleteUniversityAsync(int id);
    Task<bool> DeleteCollegeAsync(int id);
    Task<bool> DeleteDepartmentAsync(int id);
    Task<bool> DeleteBranchAsync(int id);
}
```

### UniversityService - Service Implementation (Partial)

**Description**: Implements business logic for university operations including image handling, intelligent search, and data validation.

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TansiqyV1.BLL.ModelVM;
using TansiqyV1.BLL.Services.Abstraction;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Enums;
using TansiqyV1.DAL.Helpers;
using TansiqyV1.DAL.Repo.Abstraction;

namespace TansiqyV1.BLL.Services.Implementation;

public class UniversityService : IUniversityService
{
    private readonly IUniversityRepository _universityRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly IGenericRepository<Department> _departmentRepository;
    private readonly IGenericRepository<UniversityBranch> _branchRepository;
    private readonly string _uploadsDirectory;
    private bool _uploadsDirectoryCreated = false;

    public UniversityService(
        IUniversityRepository universityRepository,
        ICollegeRepository collegeRepository,
        IGenericRepository<Department> departmentRepository,
        IGenericRepository<UniversityBranch> branchRepository,
        string uploadsPath)
    {
        _universityRepository = universityRepository;
        _collegeRepository = collegeRepository;
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _uploadsDirectory = uploadsPath;
    }

    private void EnsureUploadsDirectoryExists()
    {
        if (!_uploadsDirectoryCreated && !Directory.Exists(_uploadsDirectory))
        {
            try
            {
                Directory.CreateDirectory(_uploadsDirectory);
                _uploadsDirectoryCreated = true;
            }
            catch
            {
                // Silently ignore - we'll handle file save errors when they occur
            }
        }
    }

    private async Task<string?> SaveImageAsync(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return null;

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
            throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.");

        // Generate unique file name
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(extension))
            extension = ".jpg";

        var fileName = $"{Guid.NewGuid():N}{extension}";

        // Ensure directory exists before saving
        EnsureUploadsDirectoryExists();

        var filePath = Path.Combine(_uploadsDirectory, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        // Return relative path for database storage
        return $"/uploads/universities/{fileName}";
    }

    private void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        try
        {
            var fileName = Path.GetFileName(imagePath);
            var filePath = Path.Combine(_uploadsDirectory, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Silently ignore deletion errors
        }
    }
}
```

---

## DAL Layer

### ApplicationDbContext - Database Context

**Description**: Entity Framework DbContext that defines the database schema, entity configurations, and relationships.

```csharp
using Microsoft.EntityFrameworkCore;
using TansiqyV1.DAL.Entities;

namespace TansiqyV1.DAL.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<University> Universities { get; set; }
    public DbSet<College> Colleges { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<UniversityBranch> UniversityBranches { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<News> News { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure University
        modelBuilder.Entity<University>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.NormalizedNameAr).HasMaxLength(200);
            entity.Property(e => e.OfficialWebsite).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.Fees).HasPrecision(18, 2);
            entity.Property(e => e.LastYearCoordination).HasPrecision(18, 2);
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.HasIndex(e => e.NameAr);
            entity.HasIndex(e => e.NormalizedNameAr);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Governorate);
            entity.HasIndex(e => e.IsDeleted);
            // Composite index for common queries
            entity.HasIndex(e => new { e.Type, e.IsDeleted });
            entity.HasIndex(e => new { e.Governorate, e.IsDeleted });
        });

        // Configure College
        modelBuilder.Entity<College>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.NormalizedNameAr).HasMaxLength(200);
            entity.Property(e => e.OfficialWebsite).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.Fees).HasPrecision(18, 2);
            entity.Property(e => e.LastYearCoordination).HasPrecision(18, 2);
            
            // مصروفات بفئات
            entity.Property(e => e.FeesCategoryA).HasPrecision(18, 2);
            entity.Property(e => e.FeesCategoryB).HasPrecision(18, 2);
            entity.Property(e => e.FeesCategoryC).HasPrecision(18, 2);
            
            // مصروفات بالساعة
            entity.Property(e => e.FeesPerHour).HasPrecision(18, 2);
            entity.Property(e => e.AdditionalFees).HasPrecision(18, 2);
            
            entity.HasOne(e => e.University)
                  .WithMany(u => u.Colleges)
                  .HasForeignKey(e => e.UniversityId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.UniversityId);
            entity.HasIndex(e => e.NameAr);
            entity.HasIndex(e => e.NormalizedNameAr);
            entity.HasIndex(e => e.IsDeleted);
            // Composite index for common queries
            entity.HasIndex(e => new { e.UniversityId, e.IsDeleted });
        });

        // Configure Department
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.NormalizedNameAr).HasMaxLength(200);
            
            entity.HasOne(e => e.College)
                  .WithMany(c => c.Departments)
                  .HasForeignKey(e => e.CollegeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.CollegeId);
            entity.HasIndex(e => e.NameAr);
            entity.HasIndex(e => e.NormalizedNameAr);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.StudyType);
            // Composite index for common queries
            entity.HasIndex(e => new { e.CollegeId, e.IsDeleted });
        });

        // Configure UniversityBranch
        modelBuilder.Entity<UniversityBranch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(500);
            
            entity.HasOne(e => e.University)
                  .WithMany(u => u.Branches)
                  .HasForeignKey(e => e.UniversityId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.UniversityId);
            entity.HasIndex(e => e.IsDeleted);
            // Composite index for common queries
            entity.HasIndex(e => new { e.UniversityId, e.IsDeleted });
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Role)
                  .HasConversion<int>()
                  .HasDefaultValue(Enums.UserRole.Admin);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Role);
        });

        // Configure News
        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).IsRequired();
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.IsDeleted);
        });
    }
}
```

### BaseEntity - Base Entity Class

**Description**: Base class for all entities providing common properties like ID, timestamps, and soft delete support.

```csharp
namespace TansiqyV1.DAL.Entities;

public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

### University Entity

**Description**: Represents a university with properties for Arabic/English names, type, governorate, fees, coordination, and navigation properties to related entities.

```csharp
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class University : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    
    /// <summary>
    /// Normalized Arabic name for intelligent search.
    /// Automatically populated from NameAr using ArabicTextNormalizer.
    /// </summary>
    public string? NormalizedNameAr { get; set; }
    
    public UniversityType Type { get; set; }
    public string? OfficialWebsite { get; set; }
    public string? Location { get; set; }
    public Governorate Governorate { get; set; }
    public decimal? LastYearCoordination { get; set; } // تنسيق السنة الفائتة
    public decimal? Fees { get; set; } // المصاريف
    public string? InformationSources { get; set; } // مصادر المعلومات
    public string? Description { get; set; }
    public string? Image { get; set; } // مسار صورة الجامعة

    // Navigation Properties
    public virtual ICollection<College> Colleges { get; set; } = new List<College>();
    public virtual ICollection<UniversityBranch> Branches { get; set; } = new List<UniversityBranch>();
}
```

### College Entity

**Description**: Represents a college within a university with detailed fee structures supporting different categories and hourly fees for institutes.

```csharp
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class College : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    
    /// <summary>
    /// Normalized Arabic name for intelligent search.
    /// Automatically populated from NameAr using ArabicTextNormalizer.
    /// </summary>
    public string? NormalizedNameAr { get; set; }
    
    public int UniversityId { get; set; }
    public string? OfficialWebsite { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public decimal? Fees { get; set; } // مصروفات الكلية (قد تختلف عن الجامعة)
    public decimal? LastYearCoordination { get; set; } // تنسيق الكلية
    
    // مصروفات بفئات (للدعم الجامعات الأهلية والأجنبية)
    public decimal? FeesCategoryA { get; set; } // فئة أ / Category A
    public decimal? FeesCategoryB { get; set; } // فئة ب / Category B
    public decimal? FeesCategoryC { get; set; } // فئة ج / Category C
    
    // مصروفات بالساعة (للدعم المعاهد العالية)
    public decimal? FeesPerHour { get; set; } // مصروفات الساعة الواحدة
    public int? MinimumHoursPerSemester { get; set; } // الحد الأدنى للساعات في الفصل الدراسي
    public decimal? AdditionalFees { get; set; } // مصروفات إضافية
    
    // Navigation Properties
    public virtual University University { get; set; } = null!;
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
```

### Department Entity

**Description**: Represents a department within a college with study type information.

```csharp
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class Department : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    
    /// <summary>
    /// Normalized Arabic name for intelligent search.
    /// Automatically populated from NameAr using ArabicTextNormalizer.
    /// </summary>
    public string? NormalizedNameAr { get; set; }
    
    public int CollegeId { get; set; }
    public string? Description { get; set; }
    public StudyType? StudyType { get; set; } // نوع الدراسة المطلوب
    
    // Navigation Properties
    public virtual College College { get; set; } = null!;
}
```

### IGenericRepository - Generic Repository Interface

**Description**: Defines standard CRUD operations for any entity that inherits from BaseEntity.

```csharp
using System.Linq.Expressions;
using TansiqyV1.DAL.Entities;

namespace TansiqyV1.DAL.Repo.Abstraction;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> GetQueryable();
}
```

### IUniversityRepository - University Repository Interface

**Description**: Extends the generic repository with university-specific operations including search and intelligent Arabic search.

```csharp
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Repo.Abstraction;

public interface IUniversityRepository : IGenericRepository<University>
{
    Task<IEnumerable<University>> GetByTypeAsync(UniversityType type);
    Task<IEnumerable<University>> GetByGovernorateAsync(Governorate governorate);
    Task<IEnumerable<University>> SearchAsync(string? searchTerm, UniversityType? type, Governorate? governorate, decimal? minFees, decimal? maxFees, StudyType? studyType = null, decimal? minCoordination = null, decimal? maxCoordination = null, string? collegeName = null);
    Task<IEnumerable<University>> SearchByNameAsync(string searchTerm);
    Task<University?> GetByIdWithDetailsAsync(int id);
    Task<Dictionary<int, int>> GetBranchCountsByUniversityIdsAsync(List<int> universityIds);
    Task<Dictionary<UniversityType, int>> GetUniversityCountsByTypeAsync();
    
    // Intelligent Arabic Search Methods
    Task<IEnumerable<University>> SearchIntelligentAsync(
        string? searchTerm, 
        UniversityType? type, 
        Governorate? governorate, 
        decimal? minFees, 
        decimal? maxFees, 
        StudyType? studyType = null, 
        decimal? minCoordination = null, 
        decimal? maxCoordination = null, 
        string? collegeName = null);
    
    Task<IEnumerable<University>> SearchByNameIntelligentAsync(string searchTerm);
}
```

### GenericRepository - Generic Repository Implementation

**Description**: Provides a standard implementation of CRUD operations with soft delete support (IsDeleted flag).

```csharp
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Repo.Abstraction;

namespace TansiqyV1.DAL.Repo.Implementation;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).AsNoTracking().ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(e => !e.IsDeleted).Where(predicate).AsNoTracking().ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(e => !e.IsDeleted).FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.Now;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;
            await UpdateAsync(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate);
    }

    public virtual IQueryable<T> GetQueryable()
    {
        return _dbSet.Where(e => !e.IsDeleted).AsQueryable();
    }
}
```

### UniversityType Enum

**Description**: Defines the different types of universities in Egypt with Arabic descriptions.

```csharp
using System.ComponentModel;

namespace TansiqyV1.DAL.Enums;

public enum UniversityType
{
    [Description("جامعات حكومية")]
    Governmental = 1,
    
    [Description("جامعات خاصة")]
    Private = 2,
    
    [Description("جامعات أهلية")]
    National = 3,
    
    [Description("معاهد عالية")]
    HigherInstitute = 4,
    
    [Description("جامعات أجنبية")]
    Foreign = 5,
    
    [Description("جامعات تكنولوجية")]
    Technological = 6 ,
    [Description("جامعات ذات طبيعة خاصة ")]
    SpecialUni = 7
}
```

### Governorate Enum

**Description**: Defines all Egyptian governorates with Arabic descriptions for geographic filtering.

```csharp
using System.ComponentModel;

namespace TansiqyV1.DAL.Enums;

public enum Governorate
{
    [Description("القاهرة")]
    Cairo = 1,
    
    [Description("الإسكندرية")]
    Alexandria = 2,
    
    [Description("الجيزة")]
    Giza = 3,
    
    [Description("الشرقية")]
    Sharqia = 4,
    
    [Description("الدقهلية")]
    Dakahlia = 5,
    
    [Description("البحيرة")]
    Beheira = 6,
    
    [Description("المنوفية")]
    Monufia = 7,
    
    [Description("الغربية")]
    Gharbia = 8,
    
    [Description("كفر الشيخ")]
    KafrElSheikh = 9,
    
    [Description("القليوبية")]
    Qalyubia = 10,
    
    [Description("بني سويف")]
    BeniSuef = 11,
    
    [Description("الفيوم")]
    Fayoum = 12,
    
    [Description("المنيا")]
    Minya = 13,
    
    [Description("أسيوط")]
    Asyut = 14,
    
    [Description("سوهاج")]
    Sohag = 15,
    
    [Description("قنا")]
    Qena = 16,
    
    [Description("الأقصر")]
    Luxor = 17,
    
    [Description("أسوان")]
    Aswan = 18,
    
    [Description("البحر الأحمر")]
    RedSea = 19,
    
    [Description("الوادي الجديد")]
    NewValley = 20,
    
    [Description("مطروح")]
    Matruh = 21,
    
    [Description("شمال سيناء")]
    NorthSinai = 22,
    
    [Description("جنوب سيناء")]
    SouthSinai = 23,
    
    [Description("بورسعيد")]
    PortSaid = 24,
    
    [Description("الإسماعيلية")]
    Ismailia = 25,
    
    [Description("السويس")]
    Suez = 26,
    
    [Description("دمياط")]
    Damietta = 27
}
```

### StudyType Enum

**Description**: Defines the different study types (divisions) for Egyptian high school graduates.

```csharp
using System.ComponentModel;

namespace TansiqyV1.DAL.Enums;

public enum StudyType
{
    [Description("علم رياضة")]
    Math = 1,
    
    [Description("علم علوم")]
    Science = 2,
    
    [Description("أدبي")]
    Literary = 3,
    
    [Description("صنايع")]
    Industrial = 4,
    
    [Description("أمريكان")]
    American = 5,
    
    [Description("للجميع")]
    All = 6
}
```

---

## Architecture Summary

The Tansiqy system follows a clean three-layer architecture:

1. **API Layer**: Handles HTTP requests, authentication, authorization, and response formatting. Uses ASP.NET Core controllers with JWT authentication.

2. **BLL Layer**: Contains business logic, service interfaces and implementations. Handles data transformation, validation, image management, and coordinates between repositories.

3. **DAL Layer**: Manages data access using Entity Framework Core with Repository pattern. Includes entities, DbContext, and repository implementations with intelligent Arabic search capabilities.

Key features:
- **Soft Delete**: All entities support soft delete through `IsDeleted` flag
- **Intelligent Arabic Search**: Normalized Arabic names support flexible search with diacritic removal
- **Multi-Currency Fee Support**: Colleges support different fee categories and hourly fees
- **Comprehensive Indexing**: Database indexes optimized for common query patterns
- **JWT Authentication**: Secure API access with role-based authorization
- **Image Management**: File upload and management for university images
