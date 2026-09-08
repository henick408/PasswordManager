using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Config;
using PasswordManager.Dto;
using PasswordManager.Service;

namespace PasswordManager;

public static class Program
{

    public async static Task Main()
    {
        ServiceCollection services = new();
        services.ConfigureSupabase();
        services.AddServices();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var authService = serviceProvider.GetRequiredService<AuthService>();
    }
}
