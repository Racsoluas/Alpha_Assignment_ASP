using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class SignInDataForm
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}