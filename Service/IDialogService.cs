using MsBox.Avalonia.Enums;
using PasswordManager.Dto;

namespace PasswordManager.Service;

public interface IDialogService
{
    Task<PasswordEntry?> ShowAddEditPassword(IList<string> categories, PasswordEntry? existing = null);
    Task<bool> Confirm(string message, string title, Icon icon);
    Task ShowMessage(string message, string title, Icon icon);
    Task<string?> PromptMasterPassword(string message, string title);
}