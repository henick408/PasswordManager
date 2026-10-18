using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PasswordManager.Dto;
using PasswordManager.ViewModels;
using PasswordManager.Views.Windows;

namespace PasswordManager.Service;

public class DialogService : IDialogService
{
    
    private static Window Owner =>
        ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;
    

    public async Task<PasswordEntry?> ShowAddEditPassword(IList<string> categories, PasswordEntry? existing = null)
    {
        AddEditPasswordViewModel viewModel = new(categories, existing);
        AddEditPasswordDialog dialog = new(viewModel);

        bool saved = await dialog.ShowDialog<bool>(Owner);
        return saved ? viewModel.PasswordEntry : null;
    }

    public async Task<bool> Confirm( string message, string title, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.OkCancel, icon);
        var result = await box.ShowAsync();
        return result == ButtonResult.Ok;
    }
}