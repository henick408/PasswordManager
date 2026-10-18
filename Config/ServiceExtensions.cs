using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Model;
using PasswordManager.Repository;
using PasswordManager.Service;
using PasswordManager.ViewModels;
using AuthState = Supabase.Gotrue.Constants.AuthState;

namespace PasswordManager.Config;

public static class ServiceExtensions
{
    public static void AddDependencies(this IServiceCollection services)
    {
        services.ConfigureSupabase();
        services.AddRepositories();
        services.AddServices();
        services.AddViewModels();
    }
    
    public static void ConfigureSupabase(this IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string url = configuration["Supabase:Url"]!;
        string? publishableKey = configuration["Supabase:PublishableKey"];

        UserSession userSession = new();
        services.AddSingleton(userSession);

        Supabase.SupabaseOptions supabaseOptions = new()
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        };

        Supabase.Client supabaseClient = new(url, publishableKey, supabaseOptions);
        supabaseClient.Auth.AddStateChangedListener((_, changed) =>
        {
            switch (changed)
            {
                case AuthState.SignedOut:
                    Console.WriteLine("Wylogowano");
                    userSession.Clear();
                    break;
            }
        });
        
        services.AddSingleton(supabaseClient);
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IEncryptionService, EncryptionService>();
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddTransient<IDialogService, DialogService>();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<PasswordRepository>();
    }

    public static void AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<PasswordControlViewModel>();
        services.AddTransient<AddEditPasswordViewModel>();
        services.AddTransient<LoginRegisterViewModel>();
    }
}
