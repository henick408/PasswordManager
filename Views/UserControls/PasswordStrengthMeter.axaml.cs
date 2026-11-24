using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PasswordManager.Utils;

namespace PasswordManager.Views.UserControls;

/// <summary>
/// Coloured bar (5 segments) and label showing the strength of <see cref="Password"/>
/// (see <see cref="PasswordStrength"/>). Hidden while the password is empty.
/// </summary>
public partial class PasswordStrengthMeter : UserControl
{
    /// <summary>Password to evaluate. Bind it to the field being edited (e.g. <c>{Binding #Field.Password}</c>).</summary>
    public static readonly StyledProperty<string?> PasswordProperty =
        AvaloniaProperty.Register<PasswordStrengthMeter, string?>(nameof(Password));

    public string? Password
    {
        get => GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    private static readonly IBrush EmptySegmentBrush = new SolidColorBrush(Color.Parse("#DDDDDD"));

    /// <summary>Colour, label and number of filled segments for each level.</summary>
    private static readonly Dictionary<PasswordStrengthLevel, (IBrush Brush, string Label, int Segments)> Levels = new()
    {
        [PasswordStrengthLevel.VeryWeak] = (new SolidColorBrush(Color.Parse("#D32F2F")), "Very weak", 1),
        [PasswordStrengthLevel.Weak] = (new SolidColorBrush(Color.Parse("#F57C00")), "Weak", 2),
        [PasswordStrengthLevel.Fair] = (new SolidColorBrush(Color.Parse("#FBC02D")), "Fair", 3),
        [PasswordStrengthLevel.Strong] = (new SolidColorBrush(Color.Parse("#7CB342")), "Strong", 4),
        [PasswordStrengthLevel.VeryStrong] = (new SolidColorBrush(Color.Parse("#2E7D32")), "Very strong", 5)
    };

    public PasswordStrengthMeter()
    {
        InitializeComponent();
        Update();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PasswordProperty)
        {
            Update();
        }
    }

    /// <summary>Re-evaluates the password and repaints the segments and the label.</summary>
    private void Update()
    {
        (_, PasswordStrengthLevel level) = PasswordStrength.Evaluate(Password);
        MeterPanel.IsVisible = level != PasswordStrengthLevel.None;
        if (level == PasswordStrengthLevel.None) return;

        (IBrush brush, string label, int filledSegments) = Levels[level];
        for (int i = 0; i < Segments.Children.Count; i++)
        {
            ((Border)Segments.Children[i]).Background = i < filledSegments ? brush : EmptySegmentBrush;
        }
        LevelText.Text = label;
        LevelText.Foreground = brush;
    }
}
