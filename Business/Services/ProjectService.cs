using Data.Enteties.Notifications;
using Business.Models;
using Data.Entities;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;

public interface IProjectService
{
    Task<ProjectResult> CreateProjectAsync(AddProjectForm formData, string userId);
    Task<ProjectResult> DeleteProjectAsync(string id);
    Task<ProjectResult<Project>> GetProjectAsync(string id);
    Task<ProjectResult<IEnumerable<Project>>> GetProjectsAsync();
    Task<ProjectResult> UpdateProjectAsync(EditProjectForm formData, string userId);
}

public class ProjectService(IProjectRepository projectRepository, INotificationService notificationService, UserManager<UserEntity> userManager) : IProjectService
{
    private readonly IProjectRepository _projectRepository = projectRepository;
    private readonly INotificationService _notificationService = notificationService;
    private readonly UserManager<UserEntity> _userManager = userManager;

    public async Task<ProjectResult> CreateProjectAsync(AddProjectForm formData, string userId)
    {
        if (formData == null)
            return new ProjectResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        var projectEntity = formData.MapTo<ProjectEntity>();
        projectEntity.UserId = userId;

        var result = await _projectRepository.AddAsync(projectEntity);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ProjectResult { Succeeded = false, StatusCode = 404, Error = "User not found." };
        }

        await _notificationService.AddNotificationAsync(new NotificationEntity
        {
            Message = $"{user.FirstName} {user.LastName} created a new project: {projectEntity.ProjectName}.",
            NotificationTypeId = 2,
            Image = projectEntity.Image ?? "/Images/templates/project-template.svg"
        }, user.Id);

        return result.Succeeded
            ? new ProjectResult { Succeeded = true, StatusCode = 201 }
            : new ProjectResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<ProjectResult<IEnumerable<Project>>> GetProjectsAsync()
    {
        var response = await _projectRepository.GetAllAsync
            (
                orderByDescending: true,
                sortBy: s => s.Created,
                where: null,
                include => include.User,
                include => include.Status,
                include => include.Client
            );

        return new ProjectResult<IEnumerable<Project>> { Succeeded = true, StatusCode = 200, Result = response.Result };
    }
    public async Task<ProjectResult<Project>> GetProjectAsync(string id)
    {
        var response = await _projectRepository.GetAsync
            (
                where: x => x.Id == id,
                include => include.User,
                include => include.Status,
                include => include.Client
            );

        return response.Succeeded
            ? new ProjectResult<Project> { Succeeded = true, StatusCode = 200, Result = response.Result }
            : new ProjectResult<Project> { Succeeded = false, StatusCode = 404, Error = $"Project '{id}' was not found." };
    }

    public async Task<ProjectResult> UpdateProjectAsync(EditProjectForm formData, string userId)
    {
        if (formData == null)
            return new ProjectResult { Succeeded = false, StatusCode = 400, Error = "Invalid form data." };

        var existingProjectResult = await _projectRepository.GetEntityAsync(formData.Id);

        if (!existingProjectResult.Succeeded)
            return new ProjectResult { Succeeded = false, StatusCode = 404, Error = "Project not found." };

        var projectEntity = existingProjectResult.Result!;

        projectEntity.ProjectName = formData.ProjectName;
        projectEntity.Description = formData.Description;
        projectEntity.Budget = formData.Budget;
        projectEntity.StartDate = formData.StartDate;
        projectEntity.EndDate = formData.EndDate;
        projectEntity.ClientId = formData.ClientId;
        projectEntity.StatusId = formData.StatusId;
        projectEntity.UserId = formData.UserId;

        if (!string.IsNullOrEmpty(formData.Image))
        {
            projectEntity.Image = formData.Image;
        }

        var result = await _projectRepository.UpdateAsync(projectEntity);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ProjectResult { Succeeded = false, StatusCode = 404, Error = "User not found." };
        }

        await _notificationService.AddNotificationAsync(new NotificationEntity
        {
            Message = $"{user.FirstName} {user.LastName} updated project: {projectEntity.ProjectName}.",
            NotificationTypeId = 2,
            Image = projectEntity.Image ?? "/Images/templates/project-template.svg"
        }, user.Id);

        return result.Succeeded
            ? new ProjectResult { Succeeded = true, StatusCode = 200 }
            : new ProjectResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }



    public async Task<ProjectResult> DeleteProjectAsync(string id)
    {
        var existingEntity = await _projectRepository.GetEntityAsync(id);
        if (!existingEntity.Succeeded || existingEntity.Result == null)
        {
            return new ProjectResult
            {
                Succeeded = false,
                StatusCode = 404,
                Error = $"Project with ID '{id}' not found."
            };
        }

        var result = await _projectRepository.DeleteAsync(existingEntity.Result);
        return result.Succeeded
            ? new ProjectResult { Succeeded = true, StatusCode = 200 }
            : new ProjectResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }
}