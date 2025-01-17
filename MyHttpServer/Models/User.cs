namespace Rezka.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    
}