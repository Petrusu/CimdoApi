namespace CimdoApi.InnerClasses;

public class ModelChangePassword
{
    public string Login { get; set; }
    public string? OldPassword { get; set; }
    public string? Password { get; set; }
    public string? PasswordAgain { get; set; }
}