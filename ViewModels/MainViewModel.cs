
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLlmDesktop.Services;
using LocalLlmDesktop.Models;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace LocalLlmDesktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IChatService _chatService;
    private readonly string? _configurationError;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public MainViewModel(IChatService chatService, string? configurationError = null)
    {
        _chatService = chatService;
        _configurationError = string.IsNullOrWhiteSpace(configurationError)
            ? null
            : configurationError;
        if (_configurationError is not null)
        {
            StatusText = _configurationError;
        }
    }

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private string _responseText = string.Empty;

    [ObservableProperty]
    private string _statusText = "準備完了";

    [ObservableProperty]
    private bool _isBusy;

    partial void OnInputTextChanged(string value)
    {
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        SendMessageCommand.NotifyCanExecuteChanged();
    }

    private bool CanSendMessage()
    {
        return _configurationError is null
            && !IsBusy
            && !string.IsNullOrWhiteSpace(InputText);
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync()
    {
        var userMessage = InputText;

        IsBusy = true;
        StatusText = "Generating...";

        try
        {
            Messages.Add(new ChatMessage
            {
               Role = "user",
               Content = userMessage                
            });

            ResponseText = await _chatService.GenerateResponseAsync(
                Messages 
            );

            Messages.Add(new ChatMessage
            {
               Role = "assistant",
               Content = ResponseText 
            });

            StatusText = "done.";
        }
        catch (Exception ex)
        {
            ResponseText = string.Empty;
            StatusText = $"ERROR: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
