namespace CimdoApi.InnerClasses;

public class ModelUser
{
    public int IdUser { get; set; }

    public string? Login { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }
    public string? PasswordAgain { get; set; }
    
}