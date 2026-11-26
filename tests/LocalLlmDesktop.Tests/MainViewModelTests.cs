using LocalLlmDesktop.Models;
using LocalLlmDesktop.Services;
using LocalLlmDesktop.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LocalLlmDesktop.Tests;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_stores_configuration_warning()
    {
        var warning = "設定ファイルのURLが不正です。";

        var viewModel = new MainViewModel(
            new StubChatService(),
            configurationWarning: warning);

        Assert.Equal(warning, viewModel.ConfigurationWarning);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_normalizes_empty_warning_to_null(
        string? warning)
    {
        var viewModel = new MainViewModel(
            new StubChatService(),
            configurationWarning: warning);

        Assert.Null(viewModel.ConfigurationWarning);
    }

    [Fact]
    public void Warning_does_not_disable_message_sending()
    {
        var viewModel = new MainViewModel(
            new StubChatService(),
            configurationWarning: "設定URLが不正です。");

        viewModel.InputText = "Hello";

        Assert.True(viewModel.SendMessageCommand.CanExecute(null));
    }

    [Fact]
    public void Configuration_error_disables_message_sending()
    {
        var viewModel = new MainViewModel(
            new StubChatService(),
            configurationError: "設定エラーです。");

        viewModel.InputText = "Hello";

        Assert.False(viewModel.SendMessageCommand.CanExecute(null));
    }

    [Fact]
    public async Task Warning_remains_after_successful_message_sending()
    {
        const string warning = "設定ファイルのURLが不正です。";

        var viewModel = new MainViewModel(
            new StubChatService(),
            configurationWarning: warning);

        viewModel.InputText = "Hello";

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.Equal(warning, viewModel.ConfigurationWarning);
        Assert.Equal("done.", viewModel.StatusText);
        Assert.Equal("Test response", viewModel.ResponseText);
        Assert.Equal(2, viewModel.Messages.Count);
    }

    private sealed class StubChatService : IChatService
    {
        public Task<string> GenerateResponseAsync(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult("Test response");
        }
    }
}