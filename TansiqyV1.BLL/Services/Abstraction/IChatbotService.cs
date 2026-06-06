using TansiqyV1.BLL.ModelVM;

namespace TansiqyV1.BLL.Services.Abstraction;

public interface IChatbotService
{
    Task<ChatbotMessageResponseDto> SendMessageAsync(ChatbotMessageRequestDto request, CancellationToken cancellationToken = default);
}
