using System.Collections.ObjectModel;
using System.Windows;
using PasswordManager.Dto;
using PasswordManager.ViewModels;

namespace PasswordManager;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel = new MainViewModel();

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}