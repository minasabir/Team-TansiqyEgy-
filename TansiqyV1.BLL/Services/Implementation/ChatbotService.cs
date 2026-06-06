using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TansiqyV1.BLL.ModelVM;
using TansiqyV1.BLL.Services.Abstraction;

namespace TansiqyV1.BLL.Services.Implementation;

public class ChatbotService : IChatbotService
{
    private const string HttpClientName = "Chatbot";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatbotService> _logger;

    public ChatbotService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ChatbotService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ChatbotMessageResponseDto> SendMessageAsync(
        ChatbotMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var chatUrl = GetChatEndpointUrl();
        var apiKey = _configuration["Chatbot:SupabasePublishableKey"]
            ?? throw new InvalidOperationException("Chatbot:SupabasePublishableKey is not configured.");

        var messages = BuildMessages(request);
        var payload = JsonSerializer.Serialize(new { messages });

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, chatUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");

        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            return new ChatbotMessageResponseDto
            {
                Response = "عدد الطلبات كتير، استنى شوية وحاول تاني"
            };
        }

        if (response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
        {
            return new ChatbotMessageResponseDto
            {
                Response = "معلش يا صديقي، النظام مشغول دلوقتي شوية. ممكن تجرب تاني كمان شوية؟"
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Chatbot upstream error {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
            throw new HttpRequestException($"Chatbot service returned {(int)response.StatusCode}");
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
        if (contentType.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase))
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var text = await ParseSseStreamAsync(stream, cancellationToken);
            return new ChatbotMessageResponseDto { Response = text };
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        if (json.TryGetProperty("error", out var errorProp))
        {
            throw new HttpRequestException(errorProp.GetString() ?? "Chatbot service error");
        }

        return new ChatbotMessageResponseDto
        {
            Response = json.GetRawText()
        };
    }

    private string GetChatEndpointUrl()
    {
        var baseUrl = (_configuration["Chatbot:SupabaseUrl"] ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException("Chatbot:SupabaseUrl is not configured.");
        }

        return $"{baseUrl}/functions/v1/chat";
    }

    private static List<object> BuildMessages(ChatbotMessageRequestDto request)
    {
        var messages = new List<object>();

        if (request.Messages is { Count: > 0 })
        {
            foreach (var item in request.Messages)
            {
                var role = item.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                    ? "assistant"
                    : "user";
                messages.Add(new { role, content = item.Content });
            }
        }

        messages.Add(new { role = "user", content = request.Message.Trim() });
        return messages;
    }

    private static async Task<string> ParseSseStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
        var textBuffer = new StringBuilder();
        var responseBuilder = new StringBuilder();
        var streamDone = false;

        while (!streamDone)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (line.EndsWith('\r'))
            {
                line = line[..^1];
            }

            if (line.StartsWith(':') || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!line.StartsWith("data: ", StringComparison.Ordinal))
            {
                continue;
            }

            var jsonStr = line["data: ".Length..].Trim();
            if (jsonStr == "[DONE]")
            {
                streamDone = true;
                continue;
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonStr);
                if (doc.RootElement.TryGetProperty("choices", out var choices)
                    && choices.GetArrayLength() > 0
                    && choices[0].TryGetProperty("delta", out var delta)
                    && delta.TryGetProperty("content", out var content)
                    && content.ValueKind == JsonValueKind.String)
                {
                    var chunk = content.GetString();
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        responseBuilder.Append(chunk);
                    }
                }
            }
            catch (JsonException)
            {
                textBuffer.AppendLine(line);
            }
        }

        if (responseBuilder.Length > 0)
        {
            return responseBuilder.ToString();
        }

        return textBuffer.Length > 0 ? textBuffer.ToString() : string.Empty;
    }
}
