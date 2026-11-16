using System.Text.Json.Serialization;

namespace LocalLlmDesktop.Models;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}