
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LocalLlmDesktop.Services;
using LocalLlmDesktop.ViewModels;
using LocalLlmDesktop.Views;
using System;
using System.Net.Http;

namespace LocalLlmDesktop;

public partial class App : Application
{
    private HttpClient? _httpClient;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var resolution = ResolveOllamaEndpoint();
            IChatService chatService;
            MainViewModel viewModel;

            if (resolution.IsSuccess)
            {
                _httpClient = OllamaHttpClientFactory.Create(resolution.BaseAddress!);
                chatService = new OllamaService(_httpClient);
                viewModel = new MainViewModel(
                                chatService,
                                configurationWarning: resolution.Warning);  
                desktop.Exit += (_, _) => _httpClient.Dispose();
            }
            else
            {
                chatService = new UnavailableChatService(resolution.Error!);
                viewModel = new MainViewModel(
                                chatService,
                                configurationError: resolution.Error,
                                configurationWarning: resolution.Warning);
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static OllamaEndpointResolution ResolveOllamaEndpoint()
    {
        var commandLine = Environment.GetCommandLineArgs();
        var args = commandLine.Length > 0
            ? commandLine[1..]
            : [];
        var environmentValue = Environment.GetEnvironmentVariable(
            OllamaEndpointResolver.EnvironmentVariableName);
        var settings = new UserSettingsStore().Load();
        return OllamaEndpointResolver.Resolve(args, environmentValue, settings);
    }
}
