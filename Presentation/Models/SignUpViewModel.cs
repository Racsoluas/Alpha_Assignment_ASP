using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class SignUpViewModel
{
    [Required(ErrorMessage = "Required")]
    [Display(Name = "First Name", Prompt = "Your first name")]
    [DataType(DataType.Text)]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Last Name", Prompt = "Your last name")]
    [DataType(DataType.Text)]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email address not valid")]
    [Display(Name = "Email", Prompt = "Your email address")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [RegularExpression(@"^(0[1-9]\d{1,2}[- ]?\d{3}[- ]?\d{2}[- ]?\d{2})$|^((\+46|0046)[ ]?[1-9]\d{1,2}[- ]?\d{3}[- ]?\d{2}[- ]?\d{2})$", ErrorMessage = "Phone number not valid")]
    [Display(Name = "Phone", Prompt = "Enter yout phone number")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Job Title", Prompt = "Enter job title")]
    [DataType(DataType.Text)]
    public string? JobTitle { get; set; }

    [Required(ErrorMessage = "Required")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{8,}$", ErrorMessage = "Enter a strong password")]
    [Display(Name = "Password", Prompt = "Enter your password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password", Prompt = "Confirm your password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = null!;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Terms must be accepted.")]
    [Display(Name = "I accept Terms and Conditions")]
    public bool TermsAndConditions { get; set; }

    public string Image { get; set; } = "/Images/templates/user-template.svg";
}