using Business.Services;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
[Route("members")]
public class MembersController(IUserService userService, IWebHostEnvironment environment, RoleManager<IdentityRole> roleManager) : Controller
{
    private readonly IUserService _userService = userService;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IWebHostEnvironment _environment = environment;

    [Route("")]
    public async Task<IActionResult> Members()
    {
        var users = await _userService.GetUsersAsync();
        var roles = _roleManager.Roles
            .Select(r => new RoleModel { Id = r.Id, Name = r.Name! }).ToList();

        var model = new MembersPageViewModel
        {
            Members = users.Result?.ToList() ?? [],
            Roles = roles ?? [],
            EditMember = new EditMemberModalViewModel
            {
                Roles = roles ?? []
            }
        };

        return View(model);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost("add")]
    public async Task<IActionResult> Add(AddMemberModalViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
            );
            return BadRequest(new { success = false, errors });
        }

        string imagePath;

        if (model.MemberImage != null && model.MemberImage.Length > 0)
        {
            var fileExtension = Path.GetExtension(model.MemberImage.FileName);
            var newFileName = $"user_image_{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, newFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.MemberImage.CopyToAsync(stream);

            imagePath = $"/uploads/{newFileName}";
        }
        else
        {
            imagePath = "/Images/templates/user-template.svg";
        }

        var formData = model.MapTo<SignUpDataForm>();
        formData.Image = imagePath;

        var result = await _userService.CreateUserAsync(formData);

        return Json(new { success = result.Succeeded });
    }

    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult> Get(string id)
    {
        var result = await _userService.GetUserAsync(id);
        if (!result.Succeeded || result.SingleResult == null)
            return NotFound();

        return Json(result.SingleResult);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost("update")]
    public async Task<IActionResult> Edit(EditMemberModalViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }

        string? imageFileName = null;

        if (model.MemberImage != null && model.MemberImage.Length > 0)
        {
            var fileExtension = Path.GetExtension(model.MemberImage.FileName);
            imageFileName = $"project_image_{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, imageFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.MemberImage.CopyToAsync(stream);
        }

        var formData = model.MapTo<EditMemberForm>();
        if (imageFileName != null)
        {
            formData.Image = $"/uploads/{imageFileName}";
        }

        var result = await _userService.UpdateUserAsync(formData, model.RoleName);

        return Json(new { success = result.Succeeded });
    }

    [HttpGet("delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Error ?? "Something went wrong.";
            return RedirectToAction("Members");
        }

        TempData["SuccessMessage"] = "Member deleted successfully.";
        return RedirectToAction("Members");
    }
}