using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LocalLlmDesktop.ViewModels;

namespace LocalLlmDesktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InputTextBox.AddHandler(
            InputElement.KeyDownEvent,
            InputTextBox_KeyDown,
            RoutingStrategies.Bubble,
            handledEventsToo: true
        );
    }

    private void InputTextBox_KeyDown(
        object? sender,
        KeyEventArgs e
    )
    {
        // shift+Enterは改行扱い
        if(e.Key == Key.Enter &&
            e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return;
        }

        if(e.Key == Key.Enter)
        {
            e.Handled = true;

            if(DataContext is MainViewModel viewModel &&
                viewModel.SendMessageCommand.CanExecute(null))
            {
                viewModel.SendMessageCommand.Execute(null);
            }
        }
    }
}