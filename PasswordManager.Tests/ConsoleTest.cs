using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Core.Config;
using PasswordManager.Core.Dto;
using PasswordManager.Core.Service;

namespace PasswordManager.Tests;

public class ConsoleTest
{
    [Fact]

    public async Task GetPasswords_ManualTest()
    {
        ServiceCollection services = new();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        await services.ConfigureSupabase(configuration);

        services.AddServices();
        services.AddRepositories();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        PasswordService passwordService = serviceProvider.GetRequiredService<PasswordService>();
        AuthService authService = serviceProvider.GetRequiredService<AuthService>();

        UserRequest request = new()
        {
            Email = "test@test.com",
            Password = "testtest"
        };

        await authService.SignIn(request);

        var passwords = await passwordService.GetPasswords();

        foreach (var password in passwords)
        {
            Console.WriteLine(password);
        }
    }
}
