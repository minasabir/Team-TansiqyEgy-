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
builder.Services.AddScoped<IUniversityService, UniversityService>();
builder.Services.AddScoped<TansiqyV1.BLL.Services.Abstraction.INewsService, TansiqyV1.BLL.Services.Implementation.NewsService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Apply pending migrations
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations.");
    }
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

// Use Response Caching
app.UseResponseCaching();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
