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
