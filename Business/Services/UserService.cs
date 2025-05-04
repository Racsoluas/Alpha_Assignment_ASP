using Business.Models;
using Data.Entities;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace Business.Services;

public interface IUserService
{
    Task<UserResult> AddUserToRoleAsync(string userId, string roleName);
    Task<UserResult> CreateUserAsync(SignUpDataForm formData, string roleName = "User");
    Task<UserResult> DeleteUserAsync(string id);
    Task<UserResult> GetUserAsync(string id);
    Task<UserResult> GetUsersAsync();
    Task<UserResult> UpdateUserAsync(EditMemberForm formData, string roleName);
}

public class UserService(IUserRepository userRepository, UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task<UserResult> GetUsersAsync()
    {
        var result = await _userRepository.GetAllAsync();
        return result.MapTo<UserResult>();
    }

    public async Task<UserResult> GetUserAsync(string id)
    {
        var result = await _userRepository.GetAsync(x => x.Id == id);

        if (!result.Succeeded || result.Result == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = $"User '{id}' was not found." };

        var userModel = result.Result.MapTo<UserModel>();

        return new UserResult
        {
            Succeeded = true,
            StatusCode = 200,
            SingleResult = userModel
        };
    }

    public async Task<UserResult> AddUserToRoleAsync(string userId, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "Role does not exist." };

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "User does not exist." };

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 200 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = "Unable to add user to role." };
    }

    public async Task<UserResult> CreateUserAsync(SignUpDataForm formData, string roleName = "User")
    {
        if (formData == null)
            return new UserResult { Succeeded = false, StatusCode = 400, Error = "Form data can not be null." };

        var existsResult = await _userRepository.ExistsAsync(x => x.Email == formData.Email);
        if (existsResult.Succeeded)
            return new UserResult { Succeeded = false, StatusCode = 409, Error = "User with same email already exist." };

        try
        {
            var actualRole = (await _userRepository.ExistsAsync(_ => true)).Succeeded ? roleName : "Administrator";
            var userEntity = formData.MapTo<UserEntity>();
            userEntity.UserName = formData.Email;

            var result = await _userManager.CreateAsync(userEntity, formData.Password);
            if (result.Succeeded)
            {
                var addToRoleResult = await AddUserToRoleAsync(userEntity.Id, actualRole);

                return result.Succeeded
                    ? new UserResult { Succeeded = true, StatusCode = 201 }
                    : new UserResult { Succeeded = false, StatusCode = 201, Error = "User created but not added to role." };
            }

            return new UserResult { Succeeded = false, StatusCode = 500, Error = "Unable to create user." };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new UserResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public async Task<UserResult> UpdateUserAsync(EditMemberForm formData, string roleName)
    {
        if (formData == null)
            return new UserResult { Succeeded = false, StatusCode = 400, Error = "Form data cannot be null." };

        var user = await _userManager.FindByIdAsync(formData.Id);
        if (user == null)
            return new UserResult { Succeeded = false, StatusCode = 404, Error = "User not found." };

        try
        {
            user.FirstName = formData.FirstName;
            user.LastName = formData.LastName;
            user.Email = formData.Email;
            user.UserName = formData.Email;
            user.PhoneNumber = formData.PhoneNumber;
            user.JobTitle = formData.JobTitle;

            if (!string.IsNullOrEmpty(formData.Image))
            {
                user.Image = formData.Image;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errorMsg = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return new UserResult { Succeeded = false, StatusCode = 500, Error = $"Failed to update user. {errorMsg}" };
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (!currentRoles.Contains(roleName))
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    var errorMsg = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return new UserResult { Succeeded = false, StatusCode = 500, Error = $"Failed to remove old roles. {errorMsg}" };
                }

                var addResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!addResult.Succeeded)
                {
                    var errorMsg = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    return new UserResult { Succeeded = false, StatusCode = 500, Error = $"Failed to assign new role. {errorMsg}" };
                }
            }

            return new UserResult { Succeeded = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new UserResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public async Task<UserResult> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return new UserResult
            {
                Succeeded = false,
                StatusCode = 404,
                Error = $"User with ID '{id}' not found."
            };
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? new UserResult { Succeeded = true, StatusCode = 200 }
            : new UserResult { Succeeded = false, StatusCode = 500, Error = "Failed to delete user." };
    }
}