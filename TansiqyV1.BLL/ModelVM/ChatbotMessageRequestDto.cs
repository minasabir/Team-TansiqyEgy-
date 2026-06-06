using System.ComponentModel.DataAnnotations;

namespace TansiqyV1.BLL.ModelVM;

public class ChatbotMessageRequestDto
{
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;

    public List<ChatbotHistoryMessageDto>? Messages { get; set; }
}

public class ChatbotHistoryMessageDto
{
    [Required]
    public string Role { get; set; } = "user";

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}
