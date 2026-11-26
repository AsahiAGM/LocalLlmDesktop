
using LocalLlmDesktop.Services;
using Xunit;

namespace LocalLlmDesktop.Tests;

public class UserSettingsStoreTests
{
    [Fact]
    public void Load_returns_missing_when_file_does_not_exist()
    {
        // Arrange
        var settingsPath = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString(),
            "settings.json");

        var store = new UserSettingsStore(settingsPath);

        // Act
        var result = store.Load();

        // Assert
        Assert.False(result.Exists);
        Assert.Null(result.OllamaBaseUrl);
        Assert.Null(result.Error);
    }


    [Fact]
    public void Load_returns_settings_when_file_contains_valid_json()
    {
        // Arrange
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var settingsPath = Path.Combine(tempDirectory, "settings.json");

        try
        {
            File.WriteAllText(
                settingsPath,
                """
                {
                    "ollamaBaseUrl": "http://127.0.0.1:11434/"
                }
                """);

            var store = new UserSettingsStore(settingsPath);

            // Act
            var result = store.Load();

            // Assert
            Assert.True(result.Exists);
            Assert.Equal(
                "http://127.0.0.1:11434/",
                result.OllamaBaseUrl);
            Assert.Null(result.Error);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Load_returns_error_when_file_contains_invalid_json()
    {
        // Arrange
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var settingsPath = Path.Combine(tempDirectory, "settings.json");

        try
        {
            File.WriteAllText(settingsPath, "{ invalid json");

            var store = new UserSettingsStore(settingsPath);

            // Act
            var result = store.Load();

            // Assert
            Assert.True(result.Exists);
            Assert.Null(result.OllamaBaseUrl);
            Assert.NotNull(result.Error);
            Assert.Contains("JSONを読み取れません", result.Error);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Load_returns_error_when_file_is_empty()
    {
        // Arrange
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var settingsPath = Path.Combine(tempDirectory, "settings.json");

        try
        {
            File.WriteAllText(settingsPath, "");

            var store = new UserSettingsStore(settingsPath);

            // Act
            var result = store.Load();

            // Assert
            Assert.True(result.Exists);
            Assert.Null(result.OllamaBaseUrl);
            Assert.NotNull(result.Error);
            Assert.Contains("JSONを読み取れません", result.Error);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Load_returns_error_when_file_contains_json_null()
    {
        // Arrange
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var settingsPath = Path.Combine(tempDirectory, "settings.json");

        try
        {
            File.WriteAllText(settingsPath, "null");

            var store = new UserSettingsStore(settingsPath);

            // Act
            var result = store.Load();

            // Assert
            Assert.True(result.Exists);
            Assert.Null(result.OllamaBaseUrl);
            Assert.NotNull(result.Error);
            Assert.Contains("内容が空です", result.Error);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Load_returns_settings_when_json_has_no_ollama_base_url()
    {
        // Arrange
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var settingsPath = Path.Combine(tempDirectory, "settings.json");

        try
        {
            File.WriteAllText(settingsPath, "{}");

            var store = new UserSettingsStore(settingsPath);

            // Act
            var result = store.Load();

            // Assert
            Assert.True(result.Exists);
            Assert.Null(result.OllamaBaseUrl);
            Assert.Null(result.Error);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Load_returns_settings_when_ollama_base_url_is_empty()
    {
        // Arrange
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var settingsPath = Path.Combine(tempDirectory, "settings.json");

        try
        {
            File.WriteAllText(
                settingsPath,
                """
                {
                    "ollamaBaseUrl": ""
                }
                """);

            var store = new UserSettingsStore(settingsPath);

            // Act
            var result = store.Load();

            // Assert
            Assert.True(result.Exists);
            Assert.Equal(string.Empty, result.OllamaBaseUrl);
            Assert.Null(result.Error);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Load_returns_settings_when_ollama_base_url_is_null()
    {
        // Arrange
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDirectory);

        var settingsPath = Path.Combine(tempDirectory, "settings.json");

        try
        {
            File.WriteAllText(
                settingsPath,
                """
                {
                    "ollamaBaseUrl": null
                }
                """);

            var store = new UserSettingsStore(settingsPath);

            // Act
            var result = store.Load();

            // Assert
            Assert.True(result.Exists);
            Assert.Null(result.OllamaBaseUrl);
            Assert.Null(result.Error);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

        [Fact]
    public void BuildSettingsPath_returns_windows_path_when_windows_is_true()
    {
        // Arrange
        var localAppData = @"C:\Users\Test\AppData\Local";
        var xdgConfigHome = "/home/test/.config";
        var home = "/home/test";

        // Act
        var result = UserSettingsStore.BuildSettingsPath(
            windows: true,
            localAppData: localAppData,
            xdgConfigHome: xdgConfigHome,
            home: home);

        // Assert
        var expected = Path.Combine(
            localAppData,
            "LocalLlmDesktop",
            "settings.json");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildSettingsPath_returns_xdg_path_when_xdg_config_home_is_set()
    {
        // Arrange
        var localAppData = "/unused";
        var xdgConfigHome = "/home/test/.config";
        var home = "/home/test";

        // Act
        var result = UserSettingsStore.BuildSettingsPath(
            windows: false,
            localAppData: localAppData,
            xdgConfigHome: xdgConfigHome,
            home: home);

        // Assert
        var expected = Path.Combine(
            xdgConfigHome,
            "LocalLlmDesktop",
            "settings.json");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildSettingsPath_returns_home_config_path_when_xdg_config_home_is_not_set()
    {
        // Arrange
        var localAppData = "/unused";
        string? xdgConfigHome = null;
        var home = "/home/test";

        // Act
        var result = UserSettingsStore.BuildSettingsPath(
            windows: false,
            localAppData: localAppData,
            xdgConfigHome: xdgConfigHome,
            home: home);

        // Assert
        var expected = Path.Combine(
            home,
            ".config",
            "LocalLlmDesktop",
            "settings.json");

        Assert.Equal(expected, result);
    }
}
