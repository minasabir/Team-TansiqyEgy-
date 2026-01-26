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

