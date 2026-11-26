using System;

namespace LocalLlmDesktop.Services;

public sealed class OllamaEndpointResolution
{
    private OllamaEndpointResolution(
    Uri? baseAddress,
    string? error,
    string? warning)
    {
        BaseAddress = baseAddress;
        Error = error;
        Warning = warning;
    }

    public Uri? BaseAddress { get; }

    public string? Error { get; }

    public string? Warning { get; }

    public bool IsSuccess => BaseAddress is not null && Error is null;

    public static OllamaEndpointResolution Success(
        Uri baseAddress,
        string? warning = null)
    {
        return new OllamaEndpointResolution(baseAddress, null, warning);
    }

    public static OllamaEndpointResolution Failure(string error)
    {
        return new OllamaEndpointResolution(null, error, null);
    }
}
