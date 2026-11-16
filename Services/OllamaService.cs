using LocalLlmDesktop.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LocalLlmDesktop.Services;

public class OllamaService : IChatService
{
    private const string ModelName = "gemma4:e2b";

    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default
    )
    {
        var request = new OllamaChatRequest
        {
            Model = ModelName,
            Messages = messages,
            Stream = false
        };

        var result = await SendChatRequestAsync(
            request,
            cancellationToken
        );

        return result.Message?.Content ?? throw new InvalidOperationException(
            "Ollama returned an empty message"
        );
    }

    private async Task<OllamaChatResponse> SendChatRequestAsync(
        OllamaChatRequest request,
        CancellationToken cancellationToken)
    {
        // csオブジェクトをJSONに変換
        var json = JsonSerializer.Serialize(request);

        //　JSONをHTTPリクエスト本文に設定（usingなのでスコープを抜けたら破棄）
        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        //　HTTP POSTを非同期送信（usingなのでスコープを抜けたら破棄）
        //　サーバー停止時はここで例外
        using var response = await _httpClient.PostAsync(
            "api/chat",
            content,
            cancellationToken
        );

        // HTTPエラー応答を例外として扱う
        response.EnsureSuccessStatusCode();

        //　レスポンス本文を文字列として読む
        var responseJson = await response.Content.ReadAsStringAsync(
            cancellationToken
        );

        //　JSONをC#オブジェクトに変換
        var result = JsonSerializer.Deserialize<OllamaChatResponse>(
            responseJson
        );

        return result
                ?? throw new InvalidOperationException(
                    "Ollama returned an empty response."
                );
    }
}