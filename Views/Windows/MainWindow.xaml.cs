using System.Collections.ObjectModel;
using System.Windows;
using PasswordManager.Dto;
using PasswordManager.ViewModels;

namespace PasswordManager;

public partial class MainWindow : Window
{
    public ObservableCollection<PasswordEntry> MojePrzyciski { get; set; } = new ObservableCollection<PasswordEntry>
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
    
    public ObservableCollection<PasswordControlViewModel> Passwords { get; set; }

    public MainWindow()
    {
        InitializeComponent();


        Passwords  = new ObservableCollection<PasswordControlViewModel>(MojePrzyciski.Select(password => new PasswordControlViewModel(password)).ToList());

        this.DataContext = this;
    }
    
    
}