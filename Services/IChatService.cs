using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using LocalLlmDesktop.Models;

namespace LocalLlmDesktop.Services;

public interface IChatService
{
    Task<string> GenerateResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default
    );
}