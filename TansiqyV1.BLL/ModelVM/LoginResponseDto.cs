namespace TansiqyV1.BLL.ModelVM;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

