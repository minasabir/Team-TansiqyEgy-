# Appendix A: Backend Implementation

## Table of Contents

- [A.1 ASP.NET Core Web API Overview](#a1-aspnet-core-web-api-overview)
- [A.2 Three-Tier Architecture Design](#a2-three-tier-architecture-design)
- [A.3 Presentation Layer (API Layer)](#a3-presentation-layer-api-layer)
  - [A.3.1 Program.cs Configuration](#a31-programcs-configuration)
  - [A.3.2 Controllers](#a32-controllers)
  - [A.3.3 Authentication and Authorization](#a33-authentication-and-authorization)
- [A.4 Business Logic Layer (BLL)](#a4-business-logic-layer-bll)
  - [A.4.1 Service Interfaces](#a41-service-interfaces)
  - [A.4.2 Service Implementations](#a42-service-implementations)
  - [A.4.3 Business Rules and Validation](#a43-business-rules-and-validation)
- [A.5 Data Access Layer (DAL)](#a5-data-access-layer-dal)
  - [A.5.1 Entity Framework Core](#a51-entity-framework-core)
  - [A.5.2 Database Context](#a52-database-context)
  - [A.5.3 Database Entities](#a53-database-entities)
  - [A.5.4 Repository Pattern](#a54-repository-pattern)
  - [A.5.5 Database Relationships](#a55-database-relationships)
- [A.6 Database Design](#a6-database-design)
- [A.7 Security Implementation](#a7-security-implementation)
- [A.8 Additional Backend Features](#a8-additional-backend-features)
- [A.9 API Documentation](#a9-api-documentation)
- [A.10 Backend Architecture Summary](#a10-backend-architecture-summary)

---

## A.1 ASP.NET Core Web API Overview

The Tansiqy backend system is built using ASP.NET Core Web API, a modern, cross-platform framework for building RESTful web services. The backend serves as the central data management and business logic hub for the university information system, providing secure and efficient access to educational data through well-defined HTTP endpoints.

### Technologies and Frameworks

The backend implementation utilizes several key technologies:

- **ASP.NET Core 8.0**: The primary framework for building the Web API
- **Entity Framework Core**: Object-Relational Mapping (ORM) for database operations
- **SQL Server**: Relational database management system for data persistence
- **JWT (JSON Web Tokens)**: Stateless authentication mechanism
- **Swagger/OpenAPI**: Interactive API documentation and testing interface
- **Dependency Injection**: Built-in IoC container for managing service lifecycles

### System Responsibilities

The backend system is responsible for:

1. **Data Management**: Storing, retrieving, and managing university, college, and department information
2. **Authentication & Authorization**: Securing API endpoints through JWT-based authentication and role-based access control
3. **Business Logic Enforcement**: Implementing validation rules, search algorithms, and data transformation
4. **API Gateway**: Providing a standardized interface for frontend applications and external consumers
5. **Search Functionality**: Implementing intelligent Arabic search with diacritic normalization for improved user experience
6. **File Management**: Handling image uploads for universities and colleges
7. **Caching**: Implementing response caching to improve performance and reduce database load

[INSERT SCREENSHOT: Overall Backend Architecture Diagram]

**Figure A.1**: High-level architecture diagram showing the interaction between the backend system, database, and external clients.

---

## A.2 Three-Tier Architecture Design

The Tansiqy system implements a classic three-tier architecture pattern, which provides clear separation of concerns and promotes maintainability, scalability, and testability. This architectural approach divides the application into three distinct layers, each with specific responsibilities and well-defined interfaces.

### Architecture Layers

The three-tier architecture consists of:

1. **Presentation Layer (API Layer)**: Handles HTTP requests, authentication, and response formatting
2. **Business Logic Layer (BLL)**: Implements business rules, validation, and data transformation
3. **Data Access Layer (DAL)**: Manages database operations and data persistence

### Layer Responsibilities

**Presentation Layer**:
- Receives HTTP requests from clients
- Validates input data and authentication credentials
- Delegates business operations to the BLL
- Formats and returns HTTP responses with appropriate status codes
- Implements response caching for performance optimization
- Provides API documentation through Swagger

**Business Logic Layer**:
- Implements business rules and domain logic
- Coordinates operations between multiple repositories
- Performs data validation and transformation
- Handles image upload and file management
- Implements intelligent search algorithms
- Manages transaction boundaries and error handling

**Data Access Layer**:
- Abstracts database operations using the Repository pattern
- Implements Entity Framework Core data context
- Manages entity relationships and foreign key constraints
- Provides generic CRUD operations through base repository
- Implements custom repository methods for complex queries
- Handles soft delete mechanism for data integrity

### Benefits of Separation of Concerns

The three-tier architecture provides several key benefits:

- **Maintainability**: Each layer can be modified independently without affecting other layers
- **Testability**: Business logic can be tested in isolation from database and presentation concerns
- **Scalability**: Layers can be scaled independently based on load requirements
- **Reusability**: Business logic and data access components can be reused across different presentation layers
- **Flexibility**: Technology choices in one layer do not constrain other layers
- **Team Collaboration**: Different team members can work on different layers simultaneously

[INSERT SCREENSHOT: Solution Structure in Visual Studio]

**Figure A.2**: Visual Studio solution structure showing the three-layer organization with TansiqyV1.API, TansiqyV1.BLL, and TansiqyV1.DAL projects.

---

## A.3 Presentation Layer (API Layer)

The Presentation Layer, implemented as the TansiqyV1.API project, serves as the entry point for all client requests. This layer is responsible for handling HTTP communication, authentication, authorization, and response formatting while delegating business operations to the Business Logic Layer.

### A.3.1 Program.cs Configuration

The Program.cs file serves as the application entry point and is responsible for configuring all services, middleware, and the HTTP request pipeline. This configuration follows the ASP.NET Core minimal hosting model, which provides a clean and concise way to set up the application.

#### Purpose and Responsibilities

Program.cs performs the following critical functions:

- **Service Registration**: Configures Dependency Injection for all application services
- **Authentication Setup**: Configures JWT Bearer and Cookie authentication schemes
- **Database Configuration**: Sets up Entity Framework Core with SQL Server
- **CORS Configuration**: Enables Cross-Origin Resource Sharing for frontend integration
- **Swagger Configuration**: Sets up interactive API documentation
- **Middleware Pipeline**: Configures the order of middleware execution
- **Database Migrations**: Automatically applies database migrations on startup

#### Interaction with Other Layers

Program.cs acts as the composition root, instantiating and wiring together all components from the BLL and DAL layers. It establishes the dependency injection container that manages the lifecycle of repositories, services, and other dependencies throughout the application.

[INSERT SCREENSHOT: Program.cs Configuration Overview]

**Figure A.3**: Program.cs configuration showing service registration, authentication setup, and middleware pipeline configuration.

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

**Explanation**: The Program.cs file demonstrates the ASP.NET Core minimal hosting model. It begins by creating a WebApplicationBuilder, which provides access to configuration, logging, and dependency injection services. The configuration section sets up JSON serialization options to ignore null values and serialize enums as integers, which is important for Arabic language support. JWT authentication is configured with token validation parameters including issuer, audience, and signing key. The authentication events handler adds a custom header when tokens expire to improve client-side handling. Swagger is configured with JWT security definitions to enable authenticated API testing. CORS is set to allow all origins for development flexibility. The database context is registered with SQL Server, and repositories are registered using the Scoped lifetime to ensure proper transaction management. Services are registered with custom factory methods where complex initialization is required, such as the UniversityService which needs the uploads directory path. The middleware pipeline is configured in the correct order: CORS, HTTPS redirection, static files, response caching, authentication, authorization, and finally controller mapping. Database migrations are applied automatically on startup with error handling to prevent application failure if the database is temporarily unavailable.

### A.3.2 Controllers

Controllers in the Presentation Layer are responsible for handling HTTP requests, validating input, invoking business logic, and returning appropriate HTTP responses. Each controller is decorated with attributes that define routing, authentication requirements, and response caching policies.

#### UniversitiesController

The UniversitiesController is the primary controller for university-related operations, managing CRUD operations for universities, colleges, departments, and branches. It implements comprehensive error handling and logging to ensure reliability and maintainability.

##### Purpose and Responsibilities

The UniversitiesController handles:

- **University CRUD Operations**: Create, Read, Update, and Delete operations for universities
- **College Management**: CRUD operations for colleges within universities
- **Department Management**: CRUD operations for departments within colleges
- **Branch Management**: CRUD operations for university branches
- **Intelligent Search**: Advanced search functionality with multiple filter criteria
- **Response Caching**: Configurable caching to improve performance
- **Authorization**: Role-based access control for administrative operations

##### Interaction with Other Layers

The controller receives HTTP requests from clients, validates the input data, and delegates all business operations to the IUniversityService in the BLL layer. It does not contain any business logic itself, ensuring a clean separation of concerns. The controller transforms service responses into HTTP responses with appropriate status codes and response caching headers.

[INSERT SCREENSHOT: UniversitiesController Class Structure]

**Figure A.4**: UniversitiesController class diagram showing the main methods and their HTTP attributes.

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

**Explanation**: The UniversitiesController demonstrates RESTful API design principles with clear separation between read, create, update, and delete operations. The controller uses constructor injection to receive the IUniversityService and ILogger dependencies, following the Dependency Injection principle. Each method is decorated with appropriate HTTP verb attributes ([HttpGet], [HttpPost], [HttpPut], [HttpDelete]) and response type attributes to document expected responses. The [Authorize] attribute with Roles parameter ensures that only administrators can perform write operations, implementing role-based access control. Response caching is configured with varying durations based on the expected frequency of data changes - longer caching for read operations (300 seconds) and shorter for search operations (120 seconds). The intelligent search endpoint demonstrates flexible query parameter handling, supporting multiple optional filters that can be combined. The controller validates enum values before passing them to the service layer, providing early validation and meaningful error messages. Comprehensive error handling with try-catch blocks ensures that exceptions are logged and appropriate HTTP status codes are returned to clients. The search by name intelligent endpoint returns both universities and colleges in a single response, enabling efficient autocomplete functionality in the frontend.

#### CollegesController

The CollegesController provides dedicated endpoints for college-specific operations, offering a focused API surface for college-related functionality.

##### Purpose and Responsibilities

The CollegesController handles:

- **College Retrieval**: Get college details by ID
- **Error Handling**: Database-specific error handling with appropriate status codes
- **Logging**: Comprehensive logging for debugging and monitoring

##### Interaction with Other Layers

The controller delegates all business operations to the IUniversityService, which coordinates with the CollegeRepository in the DAL layer to retrieve college data.

[INSERT SCREENSHOT: CollegesController Implementation]

**Figure A.5**: CollegesController showing the GetById method with error handling and logging.

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

**Explanation**: The CollegesController demonstrates focused API design with a single responsibility for college operations. The controller uses dependency injection to receive the IUniversityService and ILogger, following the same pattern as other controllers. The GetById method includes XML documentation comments that are automatically picked up by Swagger to generate API documentation. Error handling is implemented with specific handling for SQL exceptions, returning a 503 Service Unavailable status code when the database is temporarily unavailable, which is more informative than a generic 500 error. Structured logging with parameters enables efficient log analysis and debugging. The method returns a 404 Not Found status when the college does not exist, following RESTful conventions for resource retrieval.

#### Authentication Controllers

The AuthController handles user authentication operations, providing secure login functionality with JWT token generation.

##### Purpose and Responsibilities

The AuthController manages:

- **User Authentication**: Validates user credentials and generates JWT tokens
- **Token Generation**: Creates signed JWT tokens with user claims
- **Security**: Implements secure password verification and token expiration

##### Interaction with Other Layers

The controller delegates authentication logic to the IAuthService in the BLL layer, which coordinates with the User repository in the DAL layer to verify credentials and generate tokens.

[INSERT SCREENSHOT: AuthController Login Endpoint]

**Figure A.6**: AuthController showing the Login endpoint with JWT token generation.

```csharp
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
```

**Explanation**: The AuthController implements secure authentication using the Login endpoint. The controller receives a LoginRequestDto containing email and password, validates the model state, and delegates authentication to the IAuthService. If authentication fails (service returns null), the controller returns a 401 Unauthorized status with a generic error message to prevent information leakage about user existence. Successful authentication returns a 200 OK status with a LoginResponseDto containing the JWT token, user information, and token expiration time. The controller uses structured logging to record authentication attempts, which is crucial for security monitoring and audit trails. The endpoint is documented with XML comments that describe the purpose, parameters, and return value, which are displayed in Swagger UI.

#### Other Controllers

The system includes additional controllers for specific functionality:

**NewsController**: Manages news and announcements with full CRUD operations. Public endpoints allow read access, while write operations require admin authorization.

**ChatbotController**: Provides an AI-powered chatbot interface for answering user questions about universities and colleges. The controller handles timeout scenarios and service unavailability gracefully.

[INSERT SCREENSHOT: Additional Controllers Overview]

**Figure A.7**: Overview of NewsController and ChatbotController showing their respective endpoints and functionality.

```csharp
// NewsController - Manages news and announcements
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
}

// ChatbotController - AI-powered chatbot interface
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
```

**Explanation**: The NewsController demonstrates a typical CRUD controller with public read endpoints and protected write endpoints. The GetAllNews endpoint uses response caching to reduce database load for frequently accessed news data. The CreateNews endpoint requires admin authorization and returns a 201 Created status with the location header pointing to the newly created resource. The ChatbotController handles external service integration with robust error handling for various failure scenarios. It accepts a CancellationToken to support request cancellation when clients disconnect. The controller handles specific exception types differently: TaskCanceledException for timeouts, InvalidOperationException for configuration errors, and HttpRequestException for upstream service failures. All these scenarios return a 503 Service Unavailable status, which is semantically correct for temporary service issues. The controller includes a fallback Arabic error message when the chatbot returns an empty response, improving user experience.

### A.3.3 Authentication and Authorization

The authentication and authorization system ensures that only authorized users can access protected resources and perform administrative operations. The system implements JWT-based stateless authentication combined with role-based authorization.

#### JWT Authentication

JWT (JSON Web Token) authentication provides a secure, stateless mechanism for user authentication. Tokens are generated upon successful login and included in the Authorization header of subsequent requests.

##### Purpose and Responsibilities

The JWT authentication system handles:

- **Token Generation**: Creates signed JWT tokens with user claims
- **Token Validation**: Verifies token signature, issuer, audience, and expiration
- **Claims Management**: Encodes user identity and role information in tokens
- **Token Expiration**: Implements configurable token lifetimes

##### Interaction with Other Layers

Authentication is configured in Program.cs using the AddJwtBearer method. The AuthService in the BLL layer generates tokens, while the JWT middleware in the API layer validates tokens on each request.

[INSERT SCREENSHOT: JWT Token Flow Diagram]

**Figure A.8**: Sequence diagram showing the JWT authentication flow from login to protected resource access.

#### Role-Based Authorization

Role-based authorization restricts access to administrative operations based on user roles. The system supports Admin and Student roles with different permission levels.

##### Purpose and Responsibilities

The authorization system manages:

- **Role Assignment**: Associates users with specific roles
- **Permission Enforcement**: Restricts access based on role membership
- **Endpoint Protection**: Applies authorization attributes to controller methods

##### Interaction with Other Layers

Authorization is enforced by the [Authorize] attribute on controller methods. The JWT middleware extracts role claims from tokens, and the authorization policy evaluates these claims against required roles.

[INSERT SCREENSHOT: Role-Based Authorization Implementation]

**Figure A.9**: Authorization attributes applied to controller methods showing role-based access control.

#### Protected Endpoints

Protected endpoints require valid authentication and appropriate authorization. Write operations (Create, Update, Delete) are restricted to administrators, while read operations are publicly accessible.

##### Purpose and Responsibilities

Protected endpoint security ensures:

- **Data Integrity**: Prevents unauthorized data modifications
- **Audit Trail**: Associates all changes with authenticated users
- **Access Control**: Enforces least privilege principle

##### Interaction with Other Layers

The [Authorize] attribute triggers the authorization middleware, which validates the JWT token and checks role claims before allowing access to the controller method.

[INSERT SCREENSHOT: Protected Endpoints Configuration]

**Figure A.10**: Example of protected endpoints with [Authorize(Roles = "Admin")] attribute applied to write operations.

---

## A.4 Business Logic Layer (BLL)

The Business Logic Layer, implemented as the TansiqyV1.BLL project, contains the application's business rules, validation logic, and data transformation operations. This layer acts as an intermediary between the Presentation Layer and Data Access Layer, ensuring that business rules are consistently enforced regardless of the presentation technology.

### A.4.1 Service Interfaces

Service interfaces define contracts for business operations, promoting loose coupling and enabling testability through dependency injection. Each interface specifies the methods that must be implemented by concrete service classes.

#### IUniversityService

The IUniversityService interface defines the contract for university-related business operations, including CRUD operations, search functionality, and intelligent Arabic search capabilities.

##### Purpose and Responsibilities

The IUniversityService interface specifies:

- **CRUD Operations**: Methods for creating, reading, updating, and deleting universities, colleges, departments, and branches
- **Search Operations**: Multiple search methods with varying filter combinations
- **Intelligent Search**: Arabic text normalization and diacritic-insensitive search
- **Data Retrieval**: Methods for retrieving related entities (colleges by university, departments by college)

##### Interaction with Other Layers

The interface is implemented by the UniversityService class in the BLL layer, which uses repositories from the DAL layer to perform data operations. Controllers in the API layer depend on this interface, enabling easy mocking for unit testing.

[INSERT SCREENSHOT: IUniversityService Interface Definition]

**Figure A.11**: IUniversityService interface showing method signatures for university operations.

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

**Explanation**: The IUniversityService interface demonstrates the Interface Segregation Principle by providing a focused contract for university-related operations. The interface is organized into logical groups: Get methods, search methods (including intelligent Arabic search), relationship retrieval methods, create methods, update methods, and delete methods. This organization improves readability and makes the interface easier to understand. The interface uses async methods throughout, which is essential for I/O-bound operations in web applications to prevent thread blocking. The intelligent search methods accept nullable parameters, enabling flexible query composition where clients can specify any combination of filters. The interface returns ViewModels (UniversityViewModel, CollegeViewModel, etc.) rather than entities, which separates the data model from the presentation model and allows for data transformation. The delete methods return boolean values indicating success, which is a common pattern for operations that don't return data.

#### Other Service Interfaces

The BLL layer includes additional service interfaces for specific domains:

**IAuthService**: Defines authentication operations including login and token generation.

**INewsService**: Defines news management operations with full CRUD functionality.

**IChatbotService**: Defines chatbot message processing with cancellation support.

[INSERT SCREENSHOT: Additional Service Interfaces]

**Figure A.12**: Overview of IAuthService, INewsService, and IChatbotService interfaces.

```csharp
// IAuthService - Authentication operations
using TansiqyV1.BLL.ModelVM;

namespace TansiqyV1.BLL.Services.Abstraction;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}

// INewsService - News management operations
using TansiqyV1.BLL.ModelVM;

namespace TansiqyV1.BLL.Services.Abstraction;

public interface INewsService
{
    Task<IEnumerable<NewsViewModel>> GetAllNewsAsync();
    Task<NewsViewModel?> GetNewsByIdAsync(int id);
    Task<NewsViewModel> CreateNewsAsync(CreateNewsDto dto);
    Task<NewsViewModel> UpdateNewsAsync(UpdateNewsDto dto);
    Task<bool> DeleteNewsAsync(int id);
}

// IChatbotService - Chatbot message processing
using TansiqyV1.BLL.ModelVM;

namespace TansiqyV1.BLL.Services.Abstraction;

public interface IChatbotService
{
    Task<ChatbotMessageResponseDto> SendMessageAsync(ChatbotMessageRequestDto request, CancellationToken cancellationToken = default);
}
```

**Explanation**: The IAuthService interface provides a single LoginAsync method that accepts credentials and returns a JWT token response. This simple interface is appropriate for authentication, which typically has a limited set of operations. The INewsService interface follows the standard CRUD pattern with methods for retrieving all news, retrieving by ID, creating, updating, and deleting news items. All methods are async to support efficient I/O operations. The IChatbotService interface includes a CancellationToken parameter, which is important for long-running external service calls to support request cancellation and prevent resource leaks. These interfaces demonstrate the Single Responsibility Principle, each focusing on a specific domain of the application.

### A.4.2 Service Implementations

Service implementations contain the actual business logic for the application. They implement the corresponding interfaces and coordinate operations between multiple repositories to fulfill business requirements.

#### UniversityService

The UniversityService implements the IUniversityService interface and contains business logic for university operations, including image management, intelligent search, and data validation.

##### Purpose and Responsibilities

The UniversityService handles:

- **Business Logic Enforcement**: Validates business rules before data persistence
- **Image Management**: Handles image upload, validation, and deletion
- **Data Transformation**: Converts entities to ViewModels for presentation
- **Search Coordination**: Orchestrates complex search operations across multiple repositories
- **Transaction Management**: Coordinates database operations to maintain data consistency

##### Interaction with Other Layers

The service depends on repository interfaces from the DAL layer (IUniversityRepository, ICollegeRepository, IGenericRepository). It is injected into controllers in the API layer, following the Dependency Inversion Principle.

[INSERT SCREENSHOT: UniversityService Class Structure]

**Figure A.13**: UniversityService class showing constructor injection and key methods.

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

**Explanation**: The UniversityService demonstrates constructor injection for all its dependencies, including repositories and the uploads directory path. The service uses multiple repositories to coordinate operations across related entities (universities, colleges, departments, branches). The EnsureUploadsDirectoryExists method ensures the uploads directory is created before attempting to save files, with error handling to prevent failures if directory creation is not permitted. The SaveImageAsync method implements comprehensive image validation, checking file type against an allowlist and generating a unique filename using GUID to prevent conflicts. The method returns a relative path for database storage, which is a best practice for portability across different deployment environments. The DeleteImage method attempts to delete image files when they are no longer needed, with error handling to prevent exceptions if the file is in use or doesn't exist. The service uses private helper methods to encapsulate common functionality, following the DRY (Don't Repeat Yourself) principle. The _uploadsDirectoryCreated flag prevents redundant directory existence checks, improving performance.

#### AuthService

The AuthService implements the IAuthService interface and handles user authentication, password verification, and JWT token generation.

##### Purpose and Responsibilities

The AuthService manages:

- **Credential Verification**: Validates user email and password against stored hashes
- **Password Security**: Uses secure password hashing and verification algorithms
- **Token Generation**: Creates signed JWT tokens with user claims
- **Session Management**: Updates last login timestamps for audit purposes

##### Interaction with Other Layers

The service depends on the IGenericRepository<User> to retrieve user data and the IConfiguration to access JWT settings. It is used by the AuthController in the API layer.

[INSERT SCREENSHOT: AuthService Implementation]

**Figure A.14**: AuthService showing password verification and JWT token generation logic.

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TansiqyV1.BLL.ModelVM;
using TansiqyV1.BLL.Services.Abstraction;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Helpers;
using TansiqyV1.DAL.Repo.Abstraction;

namespace TansiqyV1.BLL.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IGenericRepository<User> userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // Find user by email
        var user = await _userRepository.FirstOrDefaultAsync(u => 
            u.Email == request.Email && 
            u.IsActive && 
            !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: User not found - {Email}", request.Email);
            return null;
        }

        // Verify password
        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed: Invalid password - {Email}", request.Email);
            return null;
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(user);

        _logger.LogInformation("User logged in successfully: {Email}, Role: {Role}", user.Email, user.Role);

        return new LoginResponseDto
        {
            Token = token,
            Email = user.Email,
            UserId = user.Id,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes())
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"] ?? "TansiqyAPI";
        var audience = jwtSettings["Audience"] ?? "TansiqyClient";
        var expirationMinutes = GetTokenExpirationMinutes();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        if (!string.IsNullOrEmpty(user.FullName))
        {
            claims.Add(new Claim(ClaimTypes.GivenName, user.FullName));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetTokenExpirationMinutes()
    {
        var expirationMinutes = _configuration.GetSection("JwtSettings")["ExpirationMinutes"];
        if (int.TryParse(expirationMinutes, out var minutes))
        {
            return minutes;
        }
        return 60; // Default: 60 minutes
    }
}
```

**Explanation**: The AuthService demonstrates secure authentication practices. The LoginAsync method first retrieves the user by email, checking that the user is active and not soft-deleted. This prevents authentication of inactive or deleted accounts. Password verification uses the PasswordHelper.VerifyPassword method, which implements secure password hashing comparison (typically using bcrypt or PBKDF2). The method returns null for both "user not found" and "invalid password" scenarios to prevent user enumeration attacks, where attackers could determine which emails exist in the system. Upon successful authentication, the service updates the LastLoginAt timestamp for audit purposes. The GenerateJwtToken method creates a JWT token with standard claims (Subject, Email, NameIdentifier, Role) and custom claims as needed. The token is signed using HMAC-SHA256, which is a secure signing algorithm. The GetTokenExpirationMinutes method reads the expiration time from configuration with a sensible default, allowing administrators to adjust token lifetimes without code changes. Structured logging records authentication attempts with different log levels (Warning for failures, Information for successes), which is crucial for security monitoring.

#### Other Service Implementations

The BLL layer includes additional service implementations:

**NewsService**: Implements news management with CRUD operations and data validation.

**ChatbotService**: Implements chatbot message processing with external API integration and timeout handling.

[INSERT SCREENSHOT: Additional Service Implementations]

**Figure A.15**: Overview of NewsService and ChatbotService implementations.

### A.4.3 Business Rules and Validation

The Business Logic Layer enforces business rules and validation to ensure data integrity and consistency. These rules are implemented in service classes and applied before data is persisted to the database.

#### Validation Logic

Validation ensures that data meets business requirements before it is stored or processed. The system uses both attribute-based validation and programmatic validation.

##### Purpose and Responsibilities

Validation logic handles:

- **Input Validation**: Ensures data types, formats, and ranges are correct
- **Business Rule Enforcement**: Applies domain-specific rules
- **Data Consistency**: Maintains referential integrity and business invariants

##### Interaction with Other Layers

Validation is performed in service classes before calling repository methods. Invalid data results in ArgumentException or validation errors being returned to the Presentation Layer.

[INSERT SCREENSHOT: Validation Logic Implementation]

**Figure A.16**: Example of validation logic in service methods checking business rules.

#### Search Logic

The intelligent search system implements Arabic text normalization and diacritic-insensitive matching to improve search accuracy for Arabic content.

##### Purpose and Responsibilities

Search logic manages:

- **Text Normalization**: Removes diacritics and normalizes Arabic characters
- **Flexible Querying**: Supports multiple optional filter parameters
- **Performance Optimization**: Uses database indexes for efficient querying

##### Interaction with Other Layers

Search logic is implemented in service classes, which call repository methods with normalized search terms. Repositories use Entity Framework Core to generate optimized SQL queries.

[INSERT SCREENSHOT: Intelligent Search Implementation]

**Figure A.17**: Flow diagram showing the intelligent search process from user input to database query.

#### Image Processing

Image processing handles file upload validation, storage, and management for university and college images.

##### Purpose and Responsibilities

Image processing manages:

- **File Validation**: Checks file types, sizes, and formats
- **Secure Storage**: Generates unique filenames and stores files in designated directories
- **Cleanup**: Deletes unused image files to prevent storage bloat

##### Interaction with Other Layers

Image processing is implemented in the UniversityService, which saves files to the wwwroot/uploads directory and stores relative paths in the database. The API layer serves static files through the UseStaticFiles middleware.

[INSERT SCREENSHOT: Image Processing Flow]

**Figure A.18**: Sequence diagram showing image upload process from client to storage.

#### Domain Rules

Domain rules enforce business-specific constraints that cannot be expressed through simple validation attributes.

##### Purpose and Responsibilities

Domain rules ensure:

- **Data Integrity**: Maintains consistency across related entities
- **Business Invariants**: Enforces rules that must always be true
- **Cross-Entity Validation**: Validates relationships between entities

##### Interaction with Other Layers

Domain rules are implemented in service methods and may involve querying multiple repositories to validate conditions before allowing operations.

[INSERT SCREENSHOT: Domain Rules Implementation]

**Figure A.19**: Example of domain rules checking relationships between universities, colleges, and departments.

---

## A.5 Data Access Layer (DAL)

The Data Access Layer, implemented as the TansiqyV1.DAL project, is responsible for all database operations. This layer abstracts the underlying database technology using Entity Framework Core and implements the Repository pattern to provide a clean separation between business logic and data access code.

### A.5.1 Entity Framework Core

Entity Framework Core (EF Core) is the Object-Relational Mapper (ORM) used in the Tansiqy system. It provides a high-level abstraction over database operations, allowing developers to work with .NET objects instead of writing raw SQL queries.

#### Purpose and Responsibilities

EF Core handles:

- **Object-Relational Mapping**: Maps .NET classes to database tables
- **Change Tracking**: Automatically tracks entity changes and generates appropriate SQL
- **Query Translation**: Converts LINQ queries to optimized SQL statements
- **Database Migrations**: Provides a versioned approach to schema changes
- **Connection Management**: Manages database connections and transactions

#### Interaction with Other Layers

EF Core is configured in Program.cs and used throughout the DAL layer in repositories and the DbContext. The BLL layer interacts with EF Core indirectly through repository interfaces, maintaining separation of concerns.

[INSERT SCREENSHOT: Entity Framework Core Architecture]

**Figure A.20**: Diagram showing how EF Core maps entities to database tables and manages the data access lifecycle.

### A.5.2 Database Context

The ApplicationDbContext is the primary class that coordinates the data access functionality for the application. It inherits from DbContext and defines the DbSets for each entity type.

#### Purpose and Responsibilities

The ApplicationDbContext manages:

- **Entity Mapping**: Defines how entities map to database tables
- **Relationship Configuration**: Specifies relationships between entities
- **Index Configuration**: Defines database indexes for query optimization
- **Constraint Configuration**: Sets up foreign keys and other constraints

#### Interaction with Other Layers

The context is registered in Program.cs with the Scoped lifetime, ensuring that each HTTP request gets its own context instance. Repositories receive the context through dependency injection and use it to perform database operations.

[INSERT SCREENSHOT: ApplicationDbContext Configuration]

**Figure A.21**: ApplicationDbContext showing entity configurations and relationship mappings.

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

**Explanation**: The ApplicationDbContext demonstrates comprehensive Entity Framework Core configuration using the Fluent API. The OnModelCreating method is overridden to configure entity mappings, which provides more control than data annotations. Each entity is configured with property constraints such as maximum lengths, required fields, and precision for decimal values (using HasPrecision(18, 2) for financial data). Indexes are defined for frequently queried columns (NameAr, NormalizedNameAr, Type, Governorate) to improve query performance. Composite indexes are created for common query patterns (Type + IsDeleted, Governorate + IsDeleted, UniversityId + IsDeleted), which is a performance optimization for queries that filter on multiple columns. Relationships are configured using HasOne/WithMany methods to define foreign key constraints and cascade delete behavior. The User entity configuration includes a unique index on the Email field to prevent duplicate user registrations, and enum conversion for the Role property to store it as an integer in the database. The News entity includes indexes on Date and IsDeleted for efficient querying of recent news. All configurations follow EF Core conventions while explicitly defining important constraints and optimizations.

### A.5.3 Database Entities

Database entities represent the data model of the application and map to database tables. Each entity inherits from BaseEntity to provide common properties.

#### BaseEntity

The BaseEntity class provides common properties for all entities, promoting code reuse and consistency across the data model.

##### Purpose and Responsibilities

BaseEntity defines:

- **Primary Key**: Id property for entity identification
- **Audit Timestamps**: CreatedAt and UpdatedAt for tracking record lifecycle
- **Soft Delete**: IsDeleted flag for logical deletion instead of physical deletion

##### Interaction with Other Layers

All entity classes inherit from BaseEntity, ensuring consistent behavior across the data model. The GenericRepository uses the IsDeleted flag to filter out soft-deleted records from queries.

[INSERT SCREENSHOT: BaseEntity Class]

**Figure A.22**: BaseEntity class showing common properties inherited by all entities.

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

**Explanation**: The BaseEntity class demonstrates the Template Method pattern by providing a common base for all entities. The Id property serves as the primary key, using an integer type which is appropriate for most applications. The CreatedAt property is initialized to DateTime.Now using a default value, ensuring that new entities always have a creation timestamp. The UpdatedAt property is nullable, allowing entities to track when they were last modified. The IsDeleted flag implements the soft delete pattern, which is preferable to physical deletion for several reasons: it preserves data for audit trails, allows recovery of accidentally deleted records, and maintains referential integrity without complex cascade delete logic. This base class reduces code duplication and ensures consistent behavior across all entities in the system.

#### University Entity

The University entity represents educational institutions with properties for names, type, location, fees, and coordination scores.

##### Purpose and Responsibilities

The University entity stores:

- **Identification**: Arabic and English names with normalized search support
- **Classification**: University type (governmental, private, national, etc.)
- **Location**: Governorate and physical location
- **Academic Information**: Last year coordination score and fees
- **Media**: Image path for university logo/photo
- **Relationships**: Navigation properties to colleges and branches

##### Interaction with Other Layers

The University entity is used by repositories in the DAL layer, transformed to ViewModels in the BLL layer, and returned to clients through controllers in the API layer.

[INSERT SCREENSHOT: University Entity]

**Figure A.23**: University entity showing properties and navigation properties to related entities.

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

**Explanation**: The University entity demonstrates bilingual support with separate properties for Arabic (NameAr) and English (NameEn) names. The NormalizedNameAr property is used for intelligent Arabic search, storing a normalized version of the Arabic name with diacritics removed. This enables case-insensitive and diacritic-insensitive search, which is crucial for Arabic text where users may type with or without diacritics. The entity uses enum properties (UniversityType, Governorate) for type-safe classification, with the enum values stored as integers in the database. Decimal properties (LastYearCoordination, Fees) use nullable types to accommodate universities that may not have this information. Arabic comments in the code (تنسيق السنة الفائتة, المصاريف, etc.) improve code maintainability for Arabic-speaking developers. Navigation properties (Colleges, Branches) use the virtual keyword to enable Entity Framework Core lazy loading, allowing related entities to be loaded on demand. The collections are initialized in the property declaration to prevent null reference exceptions.

#### College Entity

The College entity represents faculties within universities with detailed fee structures supporting different categories and hourly fees for institutes.

##### Purpose and Responsibilities

The College entity manages:

- **Identification**: Arabic and English names with normalized search support
- **Association**: Foreign key to parent university
- **Fee Structure**: Multiple fee categories (A, B, C) and hourly fees for institutes
- **Academic Information**: Last year coordination score and study requirements
- **Relationships**: Navigation properties to university and departments

##### Interaction with Other Layers

The College entity is queried through the CollegeRepository, transformed in the UniversityService, and exposed through the UniversitiesController and CollegesController.

[INSERT SCREENSHOT: College Entity]

**Figure A.24**: College entity showing complex fee structure with categories and hourly fees.

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

**Explanation**: The College entity demonstrates support for complex fee structures to accommodate different types of educational institutions in Egypt. The standard Fees property represents regular tuition fees, while FeesCategoryA, FeesCategoryB, and FeesCategoryC support tiered pricing structures common in private and national universities where fees vary based on student categories. For higher institutes, the entity supports hourly fee structures with FeesPerHour, MinimumHoursPerSemester, and AdditionalFees properties. This flexibility allows the system to accurately represent the diverse fee structures across different types of Egyptian educational institutions. The UniversityId foreign key establishes the relationship to the parent university, and the navigation property University provides access to the parent entity. The Departments navigation property enables access to the college's departments. Arabic comments throughout the code explain the purpose of each property in the context of the Egyptian educational system. The NormalizedNameAr property, like in the University entity, enables intelligent Arabic search functionality.

#### Department Entity

The Department entity represents academic departments within colleges with study type information.

##### Purpose and Responsibilities

The Department entity contains:

- **Identification**: Arabic and English names with normalized search support
- **Association**: Foreign key to parent college
- **Study Requirements**: Study type (Math, Science, Literary, Industrial, American, All)
- **Relationships**: Navigation property to parent college

##### Interaction with Other Layers

The Department entity is accessed through the GenericRepository<Department> and included in college detail responses.

[INSERT SCREENSHOT: Department Entity]

**Figure A.25**: Department entity showing study type classification.

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

**Explanation**: The Department entity represents the finest level of granularity in the academic hierarchy (University → College → Department). The StudyType property uses a nullable enum to indicate which high school division (Math, Science, Literary, Industrial, American, or All) is required for admission to this department. This information is crucial for Egyptian students who are applying based on their high school division. The CollegeId foreign key establishes the relationship to the parent college, and the College navigation property provides access to the parent entity. Like other entities, it includes NormalizedNameAr for intelligent Arabic search and bilingual name properties. The Description property allows for additional information about the department's focus or specializations.

#### UniversityBranch Entity

The UniversityBranch entity represents branch campuses of universities located in different governorates.

##### Purpose and Responsibilities

The UniversityBranch entity stores:

- **Identification**: Arabic and English names
- **Association**: Foreign key to parent university
- **Location**: Physical location and governorate
- **Relationships**: Navigation property to parent university

##### Interaction with Other Layers

The UniversityBranch entity is managed through the GenericRepository<UniversityBranch> and included in university detail responses.

[INSERT SCREENSHOT: UniversityBranch Entity]

**Figure A.26**: UniversityBranch entity showing geographic information.

```csharp
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class UniversityBranch : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int UniversityId { get; set; }
    public string? Location { get; set; }
    public Governorate Governorate { get; set; }
    
    // Navigation Properties
    public virtual University University { get; set; } = null!;
}
```

**Explanation**: The UniversityBranch entity represents satellite campuses or branches of universities, which is common in Egypt where large universities may have multiple campuses across different governorates. The entity includes the UniversityId foreign key to establish the relationship with the parent university, and the Governorate property to specify the branch's location. This allows users to find university branches in their geographic area. The Location property provides more specific address information. The navigation property University enables access to the parent university's details. The entity follows the same pattern as other entities with bilingual name properties and inheritance from BaseEntity for common audit fields.

#### User Entity

The User entity represents system users with authentication credentials and role-based access control.

##### Purpose and Responsibilities

The User entity manages:

- **Authentication**: Email and password hash for login
- **Identification**: Full name for display
- **Authorization**: Role assignment (Admin or Student)
- **Account Status**: Active flag and last login timestamp

##### Interaction with Other Layers

The User entity is accessed through the GenericRepository<User> by the AuthService for authentication operations.

[INSERT SCREENSHOT: User Entity]

**Figure A.27**: User entity showing authentication and authorization properties.

```csharp
using System.ComponentModel.DataAnnotations;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FullName { get; set; }

    public UserRole Role { get; set; } = UserRole.Admin;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }
}
```

**Explanation**: The User entity demonstrates security best practices for user authentication and authorization. The Email property uses the [Required] and [MaxLength] data annotations to enforce validation at the entity level, which is complemented by the unique index configured in the DbContext. The PasswordHash property stores the hashed password (not the plaintext password), which is a critical security practice. The hash is generated using a secure hashing algorithm (bcrypt or PBKDF2) implemented in the PasswordHelper class. The Role property uses the UserRole enum with a default value of Admin, which is appropriate for the initial system administrator. The IsActive flag allows administrators to disable user accounts without deleting them, which is useful for temporary suspensions. The LastLoginAt timestamp tracks when users last authenticated, which is useful for security monitoring and detecting inactive accounts. The entity inherits from BaseEntity, providing audit fields (CreatedAt, UpdatedAt, IsDeleted) that are useful for user account management and compliance.

#### News Entity

The News entity represents news articles and announcements displayed on the system.

##### Purpose and Responsibilities

The News entity contains:

- **Content**: Title and description of the news item
- **Timing**: Publication date
- **Audit**: Inherited audit fields from BaseEntity

##### Interaction with Other Layers

The News entity is managed through the NewsRepository and NewsService, with public read access and admin-only write access.

[INSERT SCREENSHOT: News Entity]

**Figure A.28**: News entity showing content and timing properties.

```csharp
namespace TansiqyV1.DAL.Entities;

public class News : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
}
```

**Explanation**: The News entity is a simple entity designed to store news articles and announcements. The Title property uses a string type with a maximum length configured in the DbContext (500 characters). The Date property stores when the news was published, allowing the system to display news in chronological order. The Description property stores the full content of the news article. The entity inherits from BaseEntity, which provides Id, CreatedAt, UpdatedAt, and IsDeleted fields. The IsDeleted flag enables soft deletion of news items, which is useful for maintaining a record of deleted announcements. The simplicity of this entity reflects its straightforward purpose - storing and displaying news content without complex relationships or business rules.

### A.5.4 Repository Pattern

The Repository pattern provides an abstraction layer over the data access logic, encapsulating the details of Entity Framework Core and providing a clean interface for business logic operations.

#### IGenericRepository

The IGenericRepository interface defines standard CRUD operations that can be used with any entity type, promoting code reuse and consistency.

##### Purpose and Responsibilities

The IGenericRepository interface specifies:

- **CRUD Operations**: GetById, GetAll, Add, Update, Delete
- **Query Operations**: Find, FirstOrDefault, Exists
- **Queryable Access**: GetQueryable for complex LINQ queries

##### Interaction with Other Layers

The interface is implemented by GenericRepository<T> and used by service classes in the BLL layer. The generic type parameter T is constrained to BaseEntity, ensuring it can only be used with entity classes.

[INSERT SCREENSHOT: IGenericRepository Interface]

**Figure A.29**: IGenericRepository interface showing generic CRUD operations.

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

**Explanation**: The IGenericRepository interface demonstrates the use of C# generics to create a reusable repository contract. The generic type parameter T is constrained to BaseEntity using the where clause, ensuring that the repository can only be used with entities that inherit from BaseEntity. The interface provides a comprehensive set of CRUD operations: GetByIdAsync for retrieving a single entity by ID, GetAllAsync for retrieving all entities, FindAsync for retrieving entities matching a predicate, FirstOrDefaultAsync for retrieving the first matching entity or null, AddAsync for creating a new entity, AddRangeAsync for creating multiple entities in a single operation, UpdateAsync for modifying an entity, and DeleteAsync for soft-deleting an entity. The ExistsAsync method checks if any entity matches a predicate without retrieving the data, which is useful for validation. The GetQueryable method returns an IQueryable<T> for building complex LINQ queries, which is important for advanced scenarios that cannot be expressed through the other methods. All methods are async to support efficient I/O operations without blocking threads.

#### GenericRepository

The GenericRepository implementation provides a standard implementation of CRUD operations using Entity Framework Core, with built-in soft delete support.

##### Purpose and Responsibilities

The GenericRepository implements:

- **Standard CRUD**: All methods defined in IGenericRepository
- **Soft Delete**: Automatically filters out soft-deleted records
- **Change Tracking**: Updates the UpdatedAt timestamp on modifications
- **Error Handling**: Basic error handling for database operations

##### Interaction with Other Layers

The repository receives the ApplicationDbContext through constructor injection and uses it to perform database operations. Service classes depend on IGenericRepository<T> rather than the concrete implementation, enabling testability.

[INSERT SCREENSHOT: GenericRepository Implementation]

**Figure A.30**: GenericRepository showing implementation of CRUD operations with soft delete support.

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

**Explanation**: The GenericRepository implementation demonstrates several important data access patterns. The constructor injects the ApplicationDbContext and stores both the context and the DbSet<T> for the entity type. The DbSet is stored in a protected field to allow derived repositories to access it for custom queries. All query methods (GetByIdAsync, GetAllAsync, FindAsync, FirstOrDefaultAsync, ExistsAsync, GetQueryable) include the condition !e.IsDeleted to automatically filter out soft-deleted records, ensuring that soft-deleted entities are never returned to the business logic layer. The GetAllAsync and FindAsync methods use AsNoTracking() to disable change tracking, which improves performance for read-only operations since EF Core doesn't need to track these entities for changes. The UpdateAsync method automatically sets the UpdatedAt timestamp before saving, ensuring that this audit field is always updated. The DeleteAsync method implements soft delete by setting the IsDeleted flag to true and calling UpdateAsync, rather than physically removing the record from the database. This preserves data for audit trails and allows recovery. The AddRangeAsync method supports bulk insertion of multiple entities in a single database round-trip, which is more efficient than adding entities one at a time. The GetQueryable method returns an IQueryable that can be used to build complex queries with additional LINQ operators, with the soft delete filter already applied.

#### Custom Repositories

Custom repositories extend the generic repository with domain-specific operations that cannot be expressed through the generic interface.

##### IUniversityRepository

The IUniversityRepository interface extends IGenericRepository<University> with university-specific operations including search and intelligent Arabic search.

##### Purpose and Responsibilities

The IUniversityRepository defines:

- **Filtered Queries**: GetByTypeAsync, GetByGovernorateAsync
- **Search Operations**: SearchAsync with multiple filter parameters
- **Intelligent Search**: SearchIntelligentAsync with Arabic text normalization
- **Aggregation**: GetBranchCountsByUniversityIdsAsync, GetUniversityCountsByTypeAsync
- **Detailed Retrieval**: GetByIdWithDetailsAsync for loading related entities

##### Interaction with Other Layers

The interface is implemented by UniversityRepository and used by UniversityService in the BLL layer.

[INSERT SCREENSHOT: IUniversityRepository Interface]

**Figure A.31**: IUniversityRepository showing university-specific query methods.

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

**Explanation**: The IUniversityRepository interface demonstrates the Interface Segregation Principle by extending the generic repository with domain-specific operations. The interface inherits from IGenericRepository<University>, gaining all the standard CRUD operations while adding university-specific queries. The GetByTypeAsync and GetByGovernorateAsync methods provide filtered queries for common search scenarios. The SearchAsync method accepts multiple optional parameters, enabling flexible query composition where clients can specify any combination of filters. The SearchByNameAsync method provides a simpler search interface for name-only searches. The GetByIdWithDetailsAsync method is designed to load a university with its related entities (colleges, branches) in a single query using eager loading, which is more efficient than loading related entities separately. The GetBranchCountsByUniversityIdsAsync and GetUniversityCountsByTypeAsync methods return aggregated data as dictionaries, which is useful for dashboard displays and statistics. The intelligent search methods (SearchIntelligentAsync, SearchByNameIntelligentAsync) implement Arabic text normalization for improved search accuracy. All methods are async to support efficient I/O operations.

### A.5.5 Database Relationships

The database schema defines relationships between entities using foreign keys and navigation properties. Entity Framework Core manages these relationships and ensures referential integrity.

#### One-to-Many Relationships

The system uses one-to-many relationships to model hierarchical data structures such as University-College-Department.

##### Purpose and Responsibilities

One-to-many relationships manage:

- **Parent-Child Hierarchies**: University to Colleges, College to Departments
- **Cascade Delete**: Automatic deletion of child records when parent is deleted
- **Foreign Key Constraints**: Ensuring referential integrity

##### Interaction with Other Layers

Relationships are configured in the ApplicationDbContext using the Fluent API. Navigation properties in entities enable Entity Framework Core to automatically load related entities.

[INSERT SCREENSHOT: Database Relationship Diagram]

**Figure A.32**: Entity relationship diagram showing one-to-many relationships between University, College, and Department entities.

#### Foreign Keys

Foreign keys establish relationships between entities and ensure referential integrity in the database.

##### Purpose and Responsibilities

Foreign keys enforce:

- **Referential Integrity**: Preventing orphaned records
- **Relationship Navigation**: Enabling Entity Framework Core to load related entities
- **Cascade Operations**: Specifying behavior when parent records are deleted

##### Interaction with Other Layers

Foreign keys are defined as properties in entities (e.g., UniversityId in College) and configured in the DbContext using HasForeignKey methods.

[INSERT SCREENSHOT: Foreign Key Configuration]

**Figure A.33**: Example of foreign key configuration in ApplicationDbContext showing relationship setup.

#### Entity Configurations

Entity configurations define how entities map to database tables, including property constraints, indexes, and relationships.

##### Purpose and Responsibilities

Entity configurations specify:

- **Property Constraints**: Max lengths, required fields, data types
- **Index Definitions**: Single and composite indexes for query optimization
- **Relationship Behavior**: Cascade delete rules, foreign key constraints
- **Enum Mapping**: How enum values are stored in the database

##### Interaction with Other Layers

Configurations are defined in the OnModelCreating method of ApplicationDbContext and applied by Entity Framework Core when the database is created or migrated.

[INSERT SCREENSHOT: Entity Configuration Example]

**Figure A.34**: Example of entity configuration showing property constraints and index definitions.

---

## A.6 Database Design

The database design for the Tansiqy system follows relational database principles with normalization to ensure data integrity and minimize redundancy. The schema is designed to support the application's functional requirements while maintaining performance and scalability.

### Entity Relationship Diagram (ERD)

The Entity Relationship Diagram provides a visual representation of the database schema, showing entities, attributes, and relationships.

[INSERT SCREENSHOT: Entity Relationship Diagram (ERD)]

**Figure A.35**: Complete ERD showing all entities, their attributes, and relationships in the Tansiqy database.

### Database Structure Explanation

The database consists of the following main tables:

- **Universities**: Stores university information with type, governorate, fees, and coordination scores
- **Colleges**: Stores college information linked to universities with detailed fee structures
- **Departments**: Stores department information linked to colleges with study type requirements
- **UniversityBranches**: Stores branch campuses linked to universities with location information
- **Users**: Stores user accounts with authentication credentials and roles
- **News**: Stores news articles and announcements

All tables inherit common columns from the BaseEntity pattern: Id (primary key), CreatedAt, UpdatedAt, and IsDeleted (soft delete flag). This design provides consistent audit trails and supports data recovery.

Relationships are established through foreign keys:
- Colleges.UniversityId → Universities.Id (One-to-Many)
- Departments.CollegeId → Colleges.Id (One-to-Many)
- UniversityBranches.UniversityId → Universities.Id (One-to-Many)

Indexes are defined on frequently queried columns to optimize performance:
- NameAr and NormalizedNameAr for search operations
- Type and Governorate for filtering
- Composite indexes on (Type, IsDeleted) and (Governorate, IsDeleted) for common query patterns
- UniversityId in child tables for efficient joins

The database uses SQL Server as the relational database management system, chosen for its robustness, scalability, and integration with the .NET ecosystem.

---

## A.7 Security Implementation

The security implementation in the Tansiqy system addresses authentication, authorization, and data protection through multiple layers of defense.

### JWT Authentication

JWT (JSON Web Token) authentication provides a stateless, secure mechanism for user authentication. Tokens are cryptographically signed and contain user claims that are verified on each request.

#### Purpose and Responsibilities

JWT authentication handles:

- **Token Generation**: Creating signed tokens with user identity and role claims
- **Token Validation**: Verifying token signature and expiration on each request
- **Claims Encoding**: Storing user information in the token for authorization
- **Token Expiration**: Implementing configurable token lifetimes

#### Interaction with Other Layers

JWT authentication is configured in Program.cs using the AddJwtBearer method. The AuthService generates tokens, and the JWT middleware validates tokens before allowing access to protected endpoints.

[INSERT SCREENSHOT: JWT Authentication Flow]

**Figure A.36**: Sequence diagram showing the complete JWT authentication flow from login to protected resource access.

### Role-Based Authorization

Role-based authorization restricts access to administrative operations based on user roles. The system supports Admin and Student roles with different permission levels.

#### Purpose and Responsibilities

Role-based authorization manages:

- **Role Assignment**: Associating users with specific roles
- **Permission Enforcement**: Restricting access based on role membership
- **Endpoint Protection**: Applying authorization attributes to controller methods

#### Interaction with Other Layers

Authorization is enforced by the [Authorize] attribute on controller methods. The JWT middleware extracts role claims from tokens, and the authorization policy evaluates these claims against required roles.

[INSERT SCREENSHOT: Role-Based Authorization Implementation]

**Figure A.37**: Example of role-based authorization attributes applied to controller methods.

### Protected Endpoints

Protected endpoints require valid authentication and appropriate authorization. Write operations (Create, Update, Delete) are restricted to administrators, while read operations are publicly accessible.

#### Purpose and Responsibilities

Protected endpoint security ensures:

- **Data Integrity**: Preventing unauthorized data modifications
- **Audit Trail**: Associating all changes with authenticated users
- **Access Control**: Enforcing least privilege principle

#### Interaction with Other Layers

The [Authorize] attribute triggers the authorization middleware, which validates the JWT token and checks role claims before allowing access to the controller method.

[INSERT SCREENSHOT: Protected Endpoints Configuration]

**Figure A.38**: Example of protected endpoints with [Authorize(Roles = "Admin")] attribute applied to write operations.

---

## A.8 Additional Backend Features

The Tansiqy backend includes several advanced features that enhance functionality, performance, and user experience.

### Intelligent Arabic Search

The intelligent Arabic search system implements text normalization and diacritic-insensitive matching to improve search accuracy for Arabic content.

#### Purpose and Responsibilities

Intelligent Arabic search provides:

- **Diacritic Removal**: Normalizing Arabic text by removing diacritics (tashkeel)
- **Flexible Matching**: Supporting searches with or without diacritics
- **Performance**: Using database indexes on normalized text for efficient querying

#### Interaction with Other Layers

Search logic is implemented in service classes, which call repository methods with normalized search terms. The NormalizedNameAr property in entities stores pre-normalized text for efficient querying.

[INSERT SCREENSHOT: Intelligent Arabic Search Implementation]

**Figure A.39**: Flow diagram showing the intelligent Arabic search process from user input to database query.

### Soft Delete Mechanism

The soft delete mechanism implements logical deletion instead of physical deletion, preserving data for audit trails and enabling recovery.

#### Purpose and Responsibilities

Soft delete manages:

- **Data Preservation**: Maintaining deleted records for audit purposes
- **Recovery Capability**: Allowing restoration of accidentally deleted records
- **Referential Integrity**: Preventing cascade delete issues

#### Interaction with Other Layers

The IsDeleted flag in BaseEntity is set by the GenericRepository.DeleteAsync method. All query methods automatically filter out soft-deleted records using the condition !e.IsDeleted.

[INSERT SCREENSHOT: Soft Delete Implementation]

**Figure A.40**: Example of soft delete implementation in GenericRepository showing the IsDeleted flag usage.

### Image Upload Management

Image upload management handles file validation, storage, and serving for university and college images.

#### Purpose and Responsibilities

Image upload manages:

- **File Validation**: Checking file types, sizes, and formats
- **Secure Storage**: Generating unique filenames and storing files in designated directories
- **Cleanup**: Deleting unused image files to prevent storage bloat

#### Interaction with Other Layers

Image processing is implemented in the UniversityService, which saves files to the wwwroot/uploads directory and stores relative paths in the database. The API layer serves static files through the UseStaticFiles middleware.

[INSERT SCREENSHOT: Image Upload Flow]

**Figure A.41**: Sequence diagram showing image upload process from client to storage.

### Response Caching

Response caching improves performance by storing frequently accessed data in memory, reducing database load and response times.

#### Purpose and Responsibilities

Response caching provides:

- **Performance Improvement**: Reducing database queries for frequently accessed data
- **Load Reduction**: Decreasing server load during peak traffic
- **Configurable Duration**: Allowing different cache durations based on data volatility

#### Interaction with Other Layers

Caching is configured in Program.cs using AddResponseCaching. Controller methods use the [ResponseCache] attribute to specify cache duration and vary-by-query parameters.

[INSERT SCREENSHOT: Response Caching Configuration]

**Figure A.42**: Example of response caching attributes applied to controller methods.

### Error Handling

Comprehensive error handling ensures that exceptions are caught, logged, and returned to clients with appropriate HTTP status codes and error messages.

#### Purpose and Responsibilities

Error handling manages:

- **Exception Logging**: Recording errors for debugging and monitoring
- **Client Communication**: Returning meaningful error messages to clients
- **Status Code Mapping**: Mapping exceptions to appropriate HTTP status codes

#### Interaction with Other Layers

Error handling is implemented in controller methods using try-catch blocks. The ILogger is used to log errors with structured parameters for efficient log analysis.

[INSERT SCREENSHOT: Error Handling Implementation]

**Figure A.43**: Example of error handling in controller methods showing try-catch blocks and logging.

---

## A.9 API Documentation

The API documentation provides comprehensive information about available endpoints, parameters, responses, and authentication requirements.

### Swagger / OpenAPI

Swagger (OpenAPI) provides interactive API documentation that allows developers to explore and test the API directly from a web interface.

#### Purpose and Responsibilities

Swagger documentation offers:

- **Interactive Exploration**: Browse and test API endpoints from a web interface
- **Automatic Generation**: Documentation generated from code attributes and XML comments
- **Authentication Support**: Test authenticated endpoints with JWT tokens
- **Client SDK Generation**: Generate client libraries in multiple programming languages

#### Interaction with Other Layers

Swagger is configured in Program.cs using AddSwaggerGen and UseSwaggerUI. XML comments in controller methods are included in the documentation. JWT security is configured to allow authenticated testing.

[INSERT SCREENSHOT: Swagger UI Interface]

**Figure A.44**: Swagger UI showing the list of available API endpoints with JWT authentication support.

---

## A.10 Backend Architecture Summary

The Tansiqy backend architecture demonstrates modern software engineering practices and design patterns appropriate for a scalable, maintainable web application.

### Technologies

The backend utilizes the following key technologies:

- **ASP.NET Core 8.0**: Modern, cross-platform web framework
- **Entity Framework Core 8.0**: Object-Relational Mapper for database operations
- **SQL Server**: Relational database for data persistence
- **JWT Authentication**: Stateless authentication mechanism
- **Swagger/OpenAPI**: Interactive API documentation
- **Dependency Injection**: Built-in IoC container for service management

### Design Patterns Used

The architecture implements several established design patterns:

- **Three-Tier Architecture**: Separation of Presentation, Business Logic, and Data Access layers
- **Repository Pattern**: Abstraction over data access logic
- **Dependency Injection**: Loose coupling and testability
- **Service Layer Pattern**: Business logic encapsulation
- **DTO Pattern**: Data transfer objects for layer communication
- **Factory Pattern**: Service factory methods in Program.cs

### Architectural Decisions

Key architectural decisions include:

- **Soft Delete**: Logical deletion for data preservation and recovery
- **Intelligent Arabic Search**: Text normalization for improved search accuracy
- **JWT Authentication**: Stateless authentication for scalability
- **Response Caching**: Performance optimization for frequently accessed data
- **Generic Repository**: Code reuse for standard CRUD operations
- **View Models**: Separation of data model from presentation model

### Key Backend Features

The system provides the following key features:

- **Comprehensive CRUD Operations**: Full create, read, update, delete functionality for all entities
- **Intelligent Search**: Advanced search with multiple filters and Arabic text normalization
- **Role-Based Authorization**: Secure access control with Admin and Student roles
- **Image Management**: Secure file upload and management for university/college images
- **Multi-Currency Fee Support**: Flexible fee structures for different institution types
- **News Management**: News and announcements with public read and admin write access
- **AI Chatbot Integration**: Intelligent question answering about universities and colleges
- **Response Caching**: Performance optimization through configurable caching
- **Comprehensive Error Handling**: Robust error handling with logging and appropriate status codes
- **Interactive API Documentation**: Swagger UI for API exploration and testing

This architecture provides a solid foundation for the Tansiqy system, ensuring scalability, maintainability, and security while delivering a rich set of features to users.

---

**End of Appendix A: Backend Implementation**
