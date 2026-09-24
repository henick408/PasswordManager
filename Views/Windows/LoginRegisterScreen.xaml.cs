using System.Windows;
using PasswordManager.Dto;
using PasswordManager.Service;
using Supabase.Gotrue.Exceptions;

namespace PasswordManager.Views.Windows;

public partial class LoginRegisterScreen : Window
{
    private bool isRegister;
    private readonly AuthService authService;
    private readonly Func<MainWindow> mainWindowFactory;
    
    public LoginRegisterScreen(AuthService authService, Func<MainWindow> mainWindowFactory)
    {
        InitializeComponent();
        this.authService = authService;
        this.mainWindowFactory = mainWindowFactory;
    }

    private void LoginRegisterSwitch_OnClick(object sender, RoutedEventArgs e)
    {
        isRegister = !isRegister;
        RegisterExpander.IsExpanded = isRegister;
        AccountQuestionText.Text = isRegister ? "Already have an account? " : "Don't have an account? ";
        AccountActionText.Text = isRegister ? "Sign in" : "Create one";
        HeaderTextBlock.Text = isRegister ? "Create your account" : "Sign in";
        SignInButton.Content = isRegister ? "Create Account" : "Sign in";
    }

    private async void SignInButton_OnClick(object sender, RoutedEventArgs e)
    {
        string email = EmailBox.Text.Trim();
        string masterPassword = MasterPasswordBox.Password.Trim();
        string confirmPassword = ConfirmPasswordBox.Password.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(masterPassword))
        {
            MessageBox.Show("Insert required data", "Incorrect data", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        
        if (isRegister && !masterPassword.Equals(confirmPassword))
        {
            MessageBox.Show("Passwords do not match", "Passwords do not match", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        UserRequest request = new UserRequest(email, masterPassword);
        if (isRegister)
        {
            if (!await SignUp(request))
            {
                return;
            }
            MessageBox.Show("Account created successfully", "Account created", MessageBoxButton.OK, MessageBoxImage.None);
            return;
        }

        if (!await SignIn(request))
        {
            return;
        }
        MessageBox.Show("Signed in successfully", "Signed in", MessageBoxButton.OK, MessageBoxImage.None);

        MainWindow mainWindow = mainWindowFactory();
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        Close();
    }

    private async Task<bool> SignIn(UserRequest request)
    {
        try
        {
            await authService.SignIn(request);
            return true;
        }
        catch (GotrueException exception)
        {
            MessageBox.Show("Invalid login credentials", "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private async Task<bool> SignUp(UserRequest request)
    {
        try
        {
            await authService.SignUp(request);
            return true;
        }
        catch (GotrueException exception)
        {
            Console.WriteLine(exception.Message);
            MessageBox.Show("Account already exists", "Account create error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}