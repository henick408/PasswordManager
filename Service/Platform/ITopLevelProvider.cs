using Avalonia.Controls;

namespace PasswordManager.Service.Platform;

public interface ITopLevelProvider
{
    TopLevel? Current { get; }
}
