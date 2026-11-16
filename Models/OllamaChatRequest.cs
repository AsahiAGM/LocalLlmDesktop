using LocalLlmDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LocalLlmDesktop.Models;

public class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public IReadOnlyList<ChatMessage> Messages { get; init; } = Array.Empty<ChatMessage>();

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }
}