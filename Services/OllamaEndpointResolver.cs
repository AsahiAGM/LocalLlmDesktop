using System;
using System.Collections.Generic;

namespace LocalLlmDesktop.Services;

public static class OllamaEndpointResolver
{
    public const string ArgumentName = "--ollama-base-url";
    public const string EnvironmentVariableName = "LOCALLLM_OLLAMA_BASE_URL";
    public const string DefaultBaseUrl = "http://127.0.0.1:11434/";

    private const string ArgumentPrefix = ArgumentName + "=";

    public static readonly Uri DefaultBaseAddress = new(DefaultBaseUrl);

    public static OllamaEndpointResolution Resolve(
        IReadOnlyList<string> args,
        string? environmentValue,
        UserSettingsLoadResult settings)
    {
        if (TryReadArgument(args, out var argumentValue))
        {
            return Normalize(
                argumentValue,
                "起動引数 --ollama-base-url");
        }

        if (environmentValue is not null)
        {
            return Normalize(
                environmentValue,
                "環境変数 LOCALLLM_OLLAMA_BASE_URL");
        }

        if (settings.Error is not null)
        {
            return OllamaEndpointResolution.Success(
                DefaultBaseAddress,
                settings.Error);
        }

        if (!string.IsNullOrWhiteSpace(settings.OllamaBaseUrl))
        {
            if (!TryCreateBaseAddress(
                settings.OllamaBaseUrl,
                out var settingsAddress))
            {
                return OllamaEndpointResolution.Success(
                    DefaultBaseAddress,
                    "ユーザー設定 ollamaBaseUrl の値が無効です。http または https の絶対URLを指定してください。");
            }

            return OllamaEndpointResolution.Success(settingsAddress);
        }

        return OllamaEndpointResolution.Success(DefaultBaseAddress);
    }

    public static string[] ExcludeOwnArguments(IReadOnlyList<string> args)
    {
        var kept = new List<string>(args.Count);
        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            if (arg.StartsWith(ArgumentPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (arg.Equals(ArgumentName, StringComparison.Ordinal))
            {
                if (i + 1 < args.Count && !args[i + 1].StartsWith('-'))
                {
                    i++;
                }

                continue;
            }

            kept.Add(arg);
        }

        return kept.ToArray();
    }

    private static bool TryReadArgument(IReadOnlyList<string> args, out string? value)
    {
        var found = false;
        value = null;

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            if (arg.StartsWith(ArgumentPrefix, StringComparison.Ordinal))
            {
                found = true;
                value = arg[ArgumentPrefix.Length..];
                continue;
            }

            if (!arg.Equals(ArgumentName, StringComparison.Ordinal))
            {
                continue;
            }

            found = true;
            if (i + 1 < args.Count && !args[i + 1].StartsWith('-'))
            {
                value = args[++i];
            }
            else
            {
                value = null;
            }
        }

        return found;
    }

    private static OllamaEndpointResolution Normalize(string? raw, string source)
    {
        if (!TryCreateBaseAddress(raw, out var uri))
        {
            return OllamaEndpointResolution.Failure(
                $"{source} の値が無効です。http または https の絶対URLを指定してください。");
        }

        return OllamaEndpointResolution.Success(uri);
    }

    internal static bool TryCreateBaseAddress(string? raw, out Uri uri)
    {
        uri = null!;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        if (!Uri.TryCreate(raw.Trim(), UriKind.Absolute, out var parsed))
        {
            return false;
        }

        if (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        if (string.IsNullOrEmpty(parsed.Host))
        {
            return false;
        }

        var builder = new UriBuilder(parsed);
        var path = builder.Path;
        if (string.IsNullOrEmpty(path))
        {
            builder.Path = "/";
        }
        else if (!path.EndsWith('/'))
        {
            builder.Path = path + "/";
        }

        uri = builder.Uri;
        return true;
    }
}
