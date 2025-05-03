namespace Domain.Models;

public class EditMemberForm
{
    public string Id { get; set; } = null!;
    public string? Image { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string? JobTitle { get; set; } = null!;
}