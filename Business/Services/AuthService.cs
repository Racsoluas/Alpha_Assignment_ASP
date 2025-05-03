using Business.Models;
using Data.Entities;
using Data.Enteties.Notifications;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Business.Services;

public interface IAuthService
{
    Task<AuthResult> SignInAsync(SignInFormData formData);
    Task<AuthResult> SignOutAsync();
    Task<AuthResult> SignUpAsync(SignUpFormData formData);
}

public class AuthService(IUserService userService, SignInManager<UserEntity> signInManager, INotificationService notificationService, UserManager<UserEntity> userManager) : IAuthService
{
    private readonly IUserService _userService = userService;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly INotificationService _notificationService = notificationService;


    public async Task<AuthResult> SignInAsync(SignInFormData formData)
    {
        if (formData == null)
            return new AuthResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        var result = await _signInManager.PasswordSignInAsync(formData.Email, formData.Password, false, false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(formData.Email);
            if (user != null)
            {
                var notificationEntity = new NotificationEntity
                {
                    Message = $"{user.FirstName} {user.LastName} signed in.",
                    NotificationTypeId = 1,
                    Image = user.Image ?? "/Images/templates/user-template.svg"
                };

                await _notificationService.AddNotificationAsync(notificationEntity, user.Id);

            }
        }
        return result.Succeeded
            ? new AuthResult { Succeeded = true, StatusCode = 200 }
            : new AuthResult { Succeeded = false, StatusCode = 401, Error = "Invalid email of password." };
    }

    public async Task<AuthResult> SignUpAsync(SignUpFormData formData)
    {
        if (formData == null)
            return new AuthResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        var result = await _userService.CreateUserAsync(formData);
        return result.Succeeded
            ? new AuthResult { Succeeded = true, StatusCode = 201 }
            : new AuthResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<AuthResult> SignOutAsync()
    {
        await _signInManager.SignOutAsync();
        return new AuthResult { Succeeded = true, StatusCode = 200 };
    }
}