using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Dto;
using PasswordManager.Service;

namespace PasswordManager;

public static class Program
{
    private static async Task<ServiceProvider> ConfigureSupabase()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        string url = configuration["Supabase:Url"]!;
        string? publishableKey = configuration["Supabase:PublishableKey"];
        Supabase.SupabaseOptions supabaseOptions = new()
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        };
        Supabase.Client supabaseClient = new(url, publishableKey, supabaseOptions);

        ServiceCollection services = new();
        services.AddSingleton(supabaseClient);
        services.AddSingleton<AuthService>();

        var serviceProvider = services.BuildServiceProvider();

        await supabaseClient.InitializeAsync();
        return serviceProvider;
    }
    public static async Task Main()
    {

    }
}
