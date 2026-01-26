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

