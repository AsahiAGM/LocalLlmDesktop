using System;
using System.Net.Http;

namespace LocalLlmDesktop.Services;

public static class OllamaHttpClientFactory
{
    public static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);

    public static HttpClient Create(Uri baseAddress)
    {
        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = ConnectTimeout
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = baseAddress
        };
    }
}
