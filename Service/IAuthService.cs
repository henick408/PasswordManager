using PasswordManager.Dto;

namespace PasswordManager.Service;

public interface IAuthService
{
    public Task SignUpAsync(UserRequest request);
    public Task SignInAsync(UserRequest request);
    public void SignOut();
    public string? CurrentEmail { get; }
    public Task<bool> VerifyMasterPasswordAsync(string password);
}