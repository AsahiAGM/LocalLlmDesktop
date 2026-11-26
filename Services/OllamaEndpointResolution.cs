using System;

namespace LocalLlmDesktop.Services;

public sealed class OllamaEndpointResolution
{
    private OllamaEndpointResolution(Uri? baseAddress, string? error)
    {
        BaseAddress = baseAddress;
        Error = error;
    }

    public Uri? BaseAddress { get; }

    public string? Error { get; }

    public bool IsSuccess => BaseAddress is not null && Error is null;

    public static OllamaEndpointResolution Success(Uri baseAddress)
    {
        return new OllamaEndpointResolution(baseAddress, null);
    }

    public static OllamaEndpointResolution Failure(string error)
    {
        return new OllamaEndpointResolution(null, error);
    }
}
