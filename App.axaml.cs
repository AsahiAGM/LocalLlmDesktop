
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
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://172.27.192.1:11434/")
            };

            IChatService chatService = new OllamaService(_httpClient);

            var viewModel = new MainViewModel(chatService);

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };

            desktop.Exit += (_, _) => _httpClient.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
