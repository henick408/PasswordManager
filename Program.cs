using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Config;
using PasswordManager.Dto;
using PasswordManager.Repository;
using PasswordManager.Service;

namespace PasswordManager;

public static class Program
{

    public async static Task Main()
    {
        ServiceCollection services = new();
        services.ConfigureSupabase();
        services.AddServices();
        services.AddRepositories();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        AuthService authService = serviceProvider.GetRequiredService<AuthService>();
        PasswordRepository passwordRepository = serviceProvider.GetRequiredService<PasswordRepository>();
    }
}
