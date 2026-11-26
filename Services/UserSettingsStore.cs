using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocalLlmDesktop.Services;

public sealed class UserSettings
{
    [JsonPropertyName("ollamaBaseUrl")]
    public string? OllamaBaseUrl { get; init; }
}

public sealed class UserSettingsLoadResult
{
    private UserSettingsLoadResult(bool exists, string? ollamaBaseUrl, string? error)
    {
        Exists = exists;
        OllamaBaseUrl = ollamaBaseUrl;
        Error = error;
    }

    public bool Exists { get; }

    public string? OllamaBaseUrl { get; }

    public string? Error { get; }

    public static UserSettingsLoadResult Missing { get; } = new(false, null, null);

    public static UserSettingsLoadResult Loaded(string? ollamaBaseUrl)
    {
        return new UserSettingsLoadResult(true, ollamaBaseUrl, null);
    }

    public static UserSettingsLoadResult Failed(string error)
    {
        return new UserSettingsLoadResult(true, null, error);
    }
}

public sealed class UserSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly string _settingsPath;

    public UserSettingsStore()
        : this(GetDefaultSettingsPath())
    {
    }

    public UserSettingsStore(string settingsPath)
    {
        _settingsPath = settingsPath;
    }

    public UserSettingsLoadResult Load()
    {
        if (!File.Exists(_settingsPath))
        {
            return UserSettingsLoadResult.Missing;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions);
            if (settings is null)
            {
                return UserSettingsLoadResult.Failed(
                    $"ユーザー設定ファイルの内容が空です: {_settingsPath}");
            }

            return UserSettingsLoadResult.Loaded(settings.OllamaBaseUrl);
        }
        catch (JsonException)
        {
            return UserSettingsLoadResult.Failed(
                $"ユーザー設定ファイルのJSONを読み取れません: {_settingsPath}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return UserSettingsLoadResult.Failed(
                $"ユーザー設定ファイルを読み取れません: {_settingsPath}");
        }
    }

    public static string GetDefaultSettingsPath()
    {
        return BuildSettingsPath(
            OperatingSystem.IsWindows(),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    public static string BuildSettingsPath(
        bool windows,
        string localAppData,
        string? xdgConfigHome,
        string home)
    {
        if (windows)
        {
            return Path.Combine(localAppData, "LocalLlmDesktop", "settings.json");
        }

        if (!string.IsNullOrWhiteSpace(xdgConfigHome))
        {
            return Path.Combine(xdgConfigHome, "LocalLlmDesktop", "settings.json");
        }

        return Path.Combine(home, ".config", "LocalLlmDesktop", "settings.json");
    }
}
