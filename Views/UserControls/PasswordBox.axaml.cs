using Avalonia;
using Avalonia.Controls;

namespace PasswordManager.Views.UserControls;

public partial class PasswordBox : UserControl
{
    public static readonly StyledProperty<string?> PasswordProperty =
        AvaloniaProperty.Register<PasswordBox, string?>(
            nameof(Password));

    public string? Password
    {
        get => GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }
    
    public static readonly StyledProperty<Thickness> TextPaddingProperty =
        AvaloniaProperty.Register<PasswordBox, Thickness>(
            nameof(TextPadding));

    public Thickness TextPadding
    {
        get => GetValue(TextPaddingProperty);
        set => SetValue(TextPaddingProperty, value);
    }

    public PasswordBox()
    {
        InitializeComponent();
    }
}