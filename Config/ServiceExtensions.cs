using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Repository;
using PasswordManager.Service;

namespace PasswordManager.Config;

public static class ServiceExtensions
{
    public static void ConfigureSupabase(this IServiceCollection services)
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
        supabaseClient.InitializeAsync();
        services.AddSingleton(supabaseClient);
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<AuthService>();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<PasswordRepository>();
    }
}
