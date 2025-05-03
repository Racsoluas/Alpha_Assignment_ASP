using Business.Services;
using Data.Enteties.Notifications;
using Data.Entities;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers;

public class AuthController(IAuthService authService, SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, INotificationService notificationService) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;

    #region local identity
    public IActionResult SignUp()
    {
        ViewBag.ErrorMessage = null;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {

        if (ModelState.IsValid)
        {
            var signUpFormData = model.MapTo<SignUpFormData>();
            _ = await _authService.SignUpAsync(signUpFormData);
            return RedirectToAction("SignIn", "Auth");
        }

        ViewBag.ErrorMessage = null;
        return View(model);
    }

    public IActionResult SignIn(string returnUrl = "/projects")
    {
        ViewBag.ErrorMessage = null;
        ViewBag.ReturnUrl = returnUrl;

        if (User.Identity?.IsAuthenticated ?? false)
        {
            return Redirect(returnUrl);
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl = "/projects")
    {
        ViewBag.ErrorMessage = null;
        ViewBag.ReturnUrl = returnUrl;

        if (ModelState.IsValid)
        {
            var signInFormData = model.MapTo<SignInFormData>();
            var result = await _authService.SignInAsync(signInFormData);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(signInFormData.Email);
                if (user != null)
                {
                    var claims = await _userManager.GetClaimsAsync(user);

                    var displayName = $"{user.FirstName} {user.LastName}";
                    var existingNameClaim = claims.FirstOrDefault(c => c.Type == "DisplayName");
                    if (existingNameClaim == null || existingNameClaim.Value != displayName)
                    {
                        if (existingNameClaim != null)
                            await _userManager.RemoveClaimAsync(user, existingNameClaim);

                        await _userManager.AddClaimAsync(user, new Claim("DisplayName", displayName));
                    }

                    var displayImage = user.Image ?? "/Images/templates/user-template.svg";
                    var existingImageClaim = claims.FirstOrDefault(c => c.Type == "DisplayImage");
                    if (existingImageClaim == null || existingImageClaim.Value != displayImage)
                    {
                        if (existingImageClaim != null)
                            await _userManager.RemoveClaimAsync(user, existingImageClaim);

                        await _userManager.AddClaimAsync(user, new Claim("DisplayImage", displayImage));
                    }
                }

                return Redirect(returnUrl);
            }
        }

        ViewBag.ErrorMessage = "Incorrect email or password.";
        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync();
        return RedirectToAction("SignIn", "Auth");
    }
    #endregion
    #region external 

    [HttpPost]
    public IActionResult ExternalSignIn(string provider, string returnUrl = null!)
    {
        if (string.IsNullOrEmpty(provider))
        {
            ModelState.AddModelError("", "Invalid provider");
            return View("SignIn");
        }

        var redirectUrl = Url.Action("ExternalSignInCallback", "Auth", new { returnUrl })!;
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalSignInCallback(string returnUrl = null!, string remoteError = null!)
    {
        returnUrl ??= Url.Content("/projects");

        if (!string.IsNullOrEmpty(remoteError))
        {
            ModelState.AddModelError("", $"Error from external provider: {remoteError}");
            return View("SignIn");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return RedirectToAction("SignIn");

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);

                string firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? user.FirstName ?? "";
                string lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? user.LastName ?? "";
                string image = user.Image ?? "/Images/templates/user-template.svg";

                var displayName = $"{firstName} {lastName}";
                var existingNameClaim = claims.FirstOrDefault(c => c.Type == "DisplayName");
                if (existingNameClaim == null || existingNameClaim.Value != displayName)
                {
                    if (existingNameClaim != null)
                        await _userManager.RemoveClaimAsync(user, existingNameClaim);

                    await _userManager.AddClaimAsync(user, new Claim("DisplayName", displayName));
                }

                var existingImageClaim = claims.FirstOrDefault(c => c.Type == "DisplayImage");
                if (existingImageClaim == null || existingImageClaim.Value != image)
                {
                    if (existingImageClaim != null)
                        await _userManager.RemoveClaimAsync(user, existingImageClaim);

                    await _userManager.AddClaimAsync(user, new Claim("DisplayImage", image));
                }

                await _signInManager.SignInAsync(user, isPersistent: false);

                await _notificationService.AddNotificationAsync(new NotificationEntity
                {
                    Message = $"{user.FirstName} {user.LastName} signed in with {info.LoginProvider}.",
                    NotificationTypeId = 1,
                    Image = user.Image ?? "/Images/templates/user-template.svg"
                }, user.Id);

            }
            return LocalRedirect(returnUrl);
        }
        else
        {
            string firstName = string.Empty;
            string lastName = string.Empty;
            string image = "/Images/templates/user-template.svg";
            try
            {
                firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName)!;
                lastName = info.Principal.FindFirstValue(ClaimTypes.Surname)!;
            }
            catch { }

            string email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
            string username = $"ext_{info.LoginProvider.ToLower()}_{email}";

            var user = new UserEntity { UserName = username, Email = email, FirstName = firstName, LastName = lastName, Image = image };

            var identityResult = await _userManager.CreateAsync(user);
            if (identityResult.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);

                var claims = await _userManager.GetClaimsAsync(user);

                if (!claims.Any(c => c.Type == "DisplayName"))
                {
                    var displayName = $"{user.FirstName} {user.LastName}";
                    await _userManager.AddClaimAsync(user, new Claim("DisplayName", displayName));
                }

                if (!claims.Any(c => c.Type == "DisplayImage"))
                {
                    var displayImage = user.Image ?? "/Images/templates/user-template.svg";
                    await _userManager.AddClaimAsync(user, new Claim("DisplayImage", displayImage));
                }

                return LocalRedirect(returnUrl);
            }

            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("SignIn");
        }
    }

    #endregion
}