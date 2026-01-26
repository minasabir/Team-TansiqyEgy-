using TansiqyV1.BLL.ModelVM;

namespace TansiqyV1.BLL.Services.Abstraction;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}

