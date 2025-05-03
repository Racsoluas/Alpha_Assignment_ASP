using Business.Services;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize(Roles = "Administrator")]
[Route("clients")]
public class ClientsController(IClientService clientService, IWebHostEnvironment environment) : Controller
{
    private readonly IClientService _clientService = clientService;
    private readonly IWebHostEnvironment _environment = environment;

    [Route("")]
    public async Task<IActionResult> Clients()
    {
        var clients = await _clientService.GetClientsAsync();

        var model = new ClientsPageViewModel
        {
            Clients = clients.Result?.ToList() ?? []
        };

        return View(model);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(AddClientModalViewModel model)
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

        if (model.ClientImage != null && model.ClientImage.Length > 0)
        {
            var fileExtension = Path.GetExtension(model.ClientImage.FileName);
            var newFileName = $"client_image_{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, newFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ClientImage.CopyToAsync(stream);

            imagePath = $"/uploads/{newFileName}";
        }
        else
        {
            imagePath = "/Images/templates/client-template.svg";
        }

        var formData = model.MapTo<AddClientForm>();
        formData.Image = imagePath;

        var result = await _clientService.CreateClientAsync(formData);

        return Json(new { success = result.Succeeded });
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get(string id)
    {
        var result = await _clientService.GetClientAsync(id);

        if (!result.Succeeded || result.SingleResult == null)
            return NotFound(new { error = $"Client with ID '{id}' not found." });

        return Json(result.SingleResult);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Edit(EditClientModalViewModel model)
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

        if (model.ClientImage != null && model.ClientImage.Length > 0)
        {
            var fileExtension = Path.GetExtension(model.ClientImage.FileName);
            imageFileName = $"client_image_{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, imageFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ClientImage.CopyToAsync(stream);
        }

        var formData = model.MapTo<EditClientForm>();
        if (imageFileName != null)
        {
            formData.Image = $"/uploads/{imageFileName}";
        }

        var result = await _clientService.UpdateClientAsync(formData);

        return Json(new { success = result.Succeeded });
    }

    [HttpGet("delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _clientService.DeleteClientAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Error ?? "Something went wrong.";
            return RedirectToAction("Clients");
        }

        TempData["SuccessMessage"] = "Client deleted successfully.";
        return RedirectToAction("Clients");
    }
}