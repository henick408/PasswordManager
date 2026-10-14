using PasswordManager.Dto;

namespace PasswordManager.Service;

public interface IDialogService
{
    Task<PasswordEntry?> ShowAddEditPassword(IList<string> categories, PasswordEntry? existing = null);
}