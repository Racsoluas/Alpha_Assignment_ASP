using Business.Services;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
[Route("projects")]
public class ProjectsController(IProjectService projectService, IWebHostEnvironment environment, IClientService clientService, IUserService userService, IStatusService statusService) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly IClientService _clientService = clientService;
    private readonly IUserService _userService = userService;
    private readonly IStatusService _statusService = statusService;
    private readonly IWebHostEnvironment _environment = environment;

    [Route("")]
    public async Task<IActionResult> Projects(int? statusId)
    {
        var projects = await _projectService.GetProjectsAsync();
        var clients = await _clientService.GetClientsAsync();
        var users = await _userService.GetUsersAsync();
        var statuses = await _statusService.GetStatusesAsync();

        var projectList = projects?.Result?.ToList() ?? [];
        var filteredProjectList = projectList;

        if (statusId.HasValue)
        {
            filteredProjectList = [.. projectList.Where(p => p.StatusId == statusId.Value)];
        }

        var model = new ProjectsPageViewModel
        {
            Projects = projectList,
            FilteredProjects = filteredProjectList,
            Clients = clients?.Result?.ToList() ?? [],
            Users = users?.Result?.ToList() ?? [],
            Statuses = statuses?.Result?.ToList() ?? [],

            AddProject = new AddProjectModalViewModel
            {
                Clients = clients?.Result?.ToList() ?? [],
                Users = users?.Result?.ToList() ?? [],
                Statuses = statuses?.Result?.ToList() ?? [],
            },
            EditProject = new EditProjectModalViewModel
            {
                Clients = clients?.Result?.ToList() ?? [],
                Users = users?.Result?.ToList() ?? [],
                Statuses = statuses?.Result?.ToList() ?? [],
            }
        };

        return View(model);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(AddProjectModalViewModel model)
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

        if (model.ProjectImage != null && model.ProjectImage.Length > 0)
        {
            var fileExtension = Path.GetExtension(model.ProjectImage.FileName);
            var newFileName = $"project_image_{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, newFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ProjectImage.CopyToAsync(stream);

            imagePath = $"/uploads/{newFileName}";
        }
        else
        {
            imagePath = "/Images/templates/project-template.svg";
        }

        var formData = model.MapTo<AddProjectForm>();
        formData.Image = imagePath;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _projectService.CreateProjectAsync(formData, userId!);

        return Json(new { success = result.Succeeded });
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get(string id)
    {
        var result = await _projectService.GetProjectAsync(id);
        if (!result.Succeeded)
            return NotFound();

        return Json(result.Result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Edit(EditProjectModalViewModel model)
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

        if (model.ProjectImage != null && model.ProjectImage.Length > 0)
        {
            var fileExtension = Path.GetExtension(model.ProjectImage.FileName);
            imageFileName = $"project_image_{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, imageFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ProjectImage.CopyToAsync(stream);
        }

        var formData = model.MapTo<EditProjectForm>();
        if (imageFileName != null)
        {
            formData.Image = $"/uploads/{imageFileName}";
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _projectService.UpdateProjectAsync(formData, userId!);

        return Json(new { success = result.Succeeded });
    }


    [HttpGet("delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _projectService.DeleteProjectAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Error ?? "Something went wrong. :(";
            return RedirectToAction("Projects");
        }

        TempData["SuccessMessage"] = "Project deleted successfully. :)";
        return RedirectToAction("Projects");
    }
}