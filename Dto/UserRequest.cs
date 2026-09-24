namespace PasswordManager.Dto;

public class UserRequest
{
    public string Email { get; set; }
    public string Password { get; set; }

    public UserRequest(string email, string password)
    {
        this.Email = email;
        this.Password = password;
    }
}
