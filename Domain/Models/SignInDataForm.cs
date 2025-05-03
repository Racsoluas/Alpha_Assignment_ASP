using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class SignInFormData
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}