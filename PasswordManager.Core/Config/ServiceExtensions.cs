using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Core.Repository;
using PasswordManager.Core.Model;
using PasswordManager.Core.Service;
using AuthState = Supabase.Gotrue.Constants.AuthState;

namespace PasswordManager.Core.Config;

public static class ServiceExtensions
{
    public static async Task ConfigureSupabase(this IServiceCollection services, IConfiguration configuration)
    {
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

        await supabaseClient.InitializeAsync();
        services.AddSingleton(supabaseClient);
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<AuthService>();
        services.AddTransient<EncryptionService>();
        services.AddTransient<PasswordService>();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<PasswordRepository>();
    }
}
