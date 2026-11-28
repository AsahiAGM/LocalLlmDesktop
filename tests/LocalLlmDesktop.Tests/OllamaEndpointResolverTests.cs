using LocalLlmDesktop.Services;
using Xunit;

namespace LocalLlmDesktop.Tests;

public class OllamaEndpointResolverTests
{
    [Fact]
    public void Resolve_uses_loopback_when_nothing_is_configured()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            null,
            UserSettingsLoadResult.Missing);

        Assert.True(resolution.IsSuccess);
        Assert.Equal(
            "http://127.0.0.1:11434/",
            resolution.BaseAddress!.AbsoluteUri);
    }

    [Fact]
    public void Resolve_uses_settings_when_higher_priority_sources_are_absent()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            null,
            UserSettingsLoadResult.Loaded("http://10.0.0.8:11434"));

        Assert.Equal(
            "http://10.0.0.8:11434/",
            resolution.BaseAddress!.AbsoluteUri);
    }

    [Fact]
    public void Resolve_environment_variable_overrides_settings()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            "https://ollama.example:11434/",
            UserSettingsLoadResult.Loaded("http://10.0.0.8:11434/"));

        Assert.Equal(
            "https://ollama.example:11434/",
            resolution.BaseAddress!.AbsoluteUri);
    }

    [Fact]
    public void Resolve_argument_overrides_environment_variable()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            ["--ollama-base-url", "http://192.0.2.10:11434"],
            "http://10.0.0.8:11434/",
            UserSettingsLoadResult.Loaded("http://10.1.1.1:11434/"));

        Assert.Equal(
            "http://192.0.2.10:11434/",
            resolution.BaseAddress!.AbsoluteUri);
    }

    [Fact]
    public void Resolve_accepts_argument_equals_form_and_keeps_the_last_value()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [
                "--ollama-base-url=http://10.0.0.1:11434/",
                "--ollama-base-url=http://10.0.0.2:11434/"
            ],
            null,
            UserSettingsLoadResult.Missing);

        Assert.Equal(
            "http://10.0.0.2:11434/",
            resolution.BaseAddress!.AbsoluteUri);
    }

    [Fact]
    public void Resolve_does_not_fall_back_when_the_environment_value_is_invalid()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            "ftp://10.0.0.8:11434/",
            UserSettingsLoadResult.Loaded("http://127.0.0.1:11434/"));

        Assert.False(resolution.IsSuccess);
        Assert.Contains("環境変数 LOCALLLM_OLLAMA_BASE_URL", resolution.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_does_not_fall_back_when_the_environment_value_is_empty(string raw)
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            raw,
            UserSettingsLoadResult.Loaded("http://127.0.0.1:11434/"));

        Assert.False(resolution.IsSuccess);
        Assert.Null(resolution.BaseAddress);
        Assert.NotNull(resolution.Error);
        Assert.Contains("環境変数 LOCALLLM_OLLAMA_BASE_URL", resolution.Error);
    }

    [Fact]
    public void Resolve_rejects_a_missing_argument_value()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            ["--ollama-base-url"],
            "http://127.0.0.1:11434/",
            UserSettingsLoadResult.Missing);

        Assert.False(resolution.IsSuccess);
        Assert.Contains("起動引数 --ollama-base-url", resolution.Error);
    }

    [Fact]
    public void Resolve_uses_default_with_warning_when_settings_file_is_corrupt()
    {
        var settings = UserSettingsLoadResult.Failed("ユーザー設定ファイルのJSONを読み取れません");

        var resolution = OllamaEndpointResolver.Resolve([], null, settings);

        Assert.True(resolution.IsSuccess);
        Assert.Equal(
            OllamaEndpointResolver.DefaultBaseAddress,
            resolution.BaseAddress);
        Assert.Null(resolution.Error);
        Assert.Equal(settings.Error, resolution.Warning);
    }

    [Fact]
    public void Resolve_ignores_corrupt_settings_file_when_argument_is_valid()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            ["--ollama-base-url", "http://192.0.2.10:11434"],
            null,
            UserSettingsLoadResult.Failed("壊れている"));

        Assert.True(resolution.IsSuccess);
        Assert.Equal(
            "http://192.0.2.10:11434/",
            resolution.BaseAddress!.AbsoluteUri);
        Assert.Null(resolution.Warning);
    }

    [Fact]
    public void Resolve_uses_default_with_warning_when_settings_url_is_invalid()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            null,
            UserSettingsLoadResult.Loaded("ftp://10.0.0.8:11434/"));

        Assert.True(resolution.IsSuccess);
        Assert.Equal(
            OllamaEndpointResolver.DefaultBaseAddress,
            resolution.BaseAddress);
        Assert.Null(resolution.Error);
        Assert.NotNull(resolution.Warning);
        Assert.Contains("ユーザー設定 ollamaBaseUrl", resolution.Warning);
    }

    [Fact]
    public void Resolve_does_not_set_warning_when_settings_url_is_valid()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            null,
            UserSettingsLoadResult.Loaded("http://10.0.0.8:11434/"));

        Assert.True(resolution.IsSuccess);
        Assert.Null(resolution.Error);
        Assert.Null(resolution.Warning);
    }

    [Fact]
    public void Resolve_does_not_fall_back_when_argument_value_is_invalid()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            ["--ollama-base-url", "ftp://192.0.2.10:11434"],
            "http://127.0.0.1:11434/",
            UserSettingsLoadResult.Loaded("http://10.0.0.8:11434/"));

        Assert.False(resolution.IsSuccess);
        Assert.Null(resolution.BaseAddress);
        Assert.NotNull(resolution.Error);
        Assert.Contains("起動引数 --ollama-base-url", resolution.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_uses_default_without_warning_when_settings_url_is_empty(string? raw)
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            null,
            UserSettingsLoadResult.Loaded(raw));

        Assert.True(resolution.IsSuccess);
        Assert.Equal(
            OllamaEndpointResolver.DefaultBaseAddress,
            resolution.BaseAddress);
        Assert.Null(resolution.Error);
        Assert.Null(resolution.Warning);
    }

    [Fact]
    public void Resolve_ignores_a_corrupt_settings_file_when_the_environment_is_valid()
    {
        var resolution = OllamaEndpointResolver.Resolve(
            [],
            "http://127.0.0.1:11435/",
            UserSettingsLoadResult.Failed("壊れている"));

        Assert.Equal(
            "http://127.0.0.1:11435/",
            resolution.BaseAddress!.AbsoluteUri);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("127.0.0.1:11434")]
    [InlineData("file:///tmp/ollama")]
    [InlineData("http://")]
    public void TryCreateBaseAddress_rejects_values_that_are_not_absolute_http_urls(string raw)
    {
        Assert.False(OllamaEndpointResolver.TryCreateBaseAddress(raw, out _));
    }

    [Fact]
    public void ExcludeOwnArguments_removes_the_endpoint_switch_and_keeps_other_arguments()
    {
        var kept = OllamaEndpointResolver.ExcludeOwnArguments(
        [
            "--foo",
            "--ollama-base-url",
            "http://127.0.0.1:11434/",
            "--ollama-base-url=http://10.0.0.2:11434/",
            "--bar"
        ]);

        Assert.Equal(["--foo", "--bar"], kept);
    }
}
