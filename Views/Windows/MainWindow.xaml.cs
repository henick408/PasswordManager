using System.Collections.ObjectModel;
using System.Windows;
using PasswordManager.Dto;

namespace PasswordManager;

public partial class MainWindow : Window
{
    public ObservableCollection<PasswordEntry> MojePrzyciski { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        // Tworzymy listę i dodajemy do niej elementy
        MojePrzyciski = new ObservableCollection<PasswordEntry>
        {
            new PasswordEntry
            {
                Url = "github.com",
                Name = "Github",
                Username = "github@mail.com"
            },
            new PasswordEntry
            {
                Url = "google.com",
                Name = "Google",
                Username = "google@mail.com"
            },
            new PasswordEntry
            {
                Url = "facebook.com",
                Name = "Facebook",
                Username = "facebook@mail.com"
            },
            new PasswordEntry
            {
                Url = "usos.com",
                Name = "Usos",
                Username = "usos@mail.com"
            },
        };

        // Ustawiamy DataContext, aby XAML widział naszą listę
        this.DataContext = this;
    }
    
    
}