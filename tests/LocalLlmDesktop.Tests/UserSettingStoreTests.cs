
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

}
