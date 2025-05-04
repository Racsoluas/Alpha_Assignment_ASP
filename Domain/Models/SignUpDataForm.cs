namespace Domain.Models;

public class SignUpDataForm
{
    public string? Image { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public string Password { get; set; } = null!;
}