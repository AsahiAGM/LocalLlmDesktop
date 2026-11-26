using LocalLlmDesktop.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LocalLlmDesktop.Services;

public sealed class UnavailableChatService : IChatService
{
    private readonly string _message;

    public UnavailableChatService(string message)
    {
        _message = message;
    }

    public Task<string> GenerateResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(_message);
    }
}
