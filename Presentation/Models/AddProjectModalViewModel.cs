using Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class AddProjectModalViewModel
{
    [Display(Name = "Project Image", Prompt = "Select an image")]
    [DataType(DataType.Upload)]
    public IFormFile? ProjectImage { get; set; }

    [Display(Name = "Project Name", Prompt = "Project Name")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string ProjectName { get; set; } = null!;

    [Display(Name = "Description", Prompt = "Type something")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Required")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Budget", Prompt = "0")]
    [DataType(DataType.Currency)]
    public decimal? Budget { get; set; }

    [Display(Name = "Client Name")]
    [Required(ErrorMessage = "Required")]
    public string ClientId { get; set; } = null!;
    [BindNever]
    public IEnumerable<ClientModel>? Clients { get; set; }

    [Display(Name = "Member")]
    [Required(ErrorMessage = "Required")]
    public string UserId { get; set; } = null!;
    [BindNever]
    public IEnumerable<UserModel>? Users { get; set; }

    [Display(Name = "Project Status")]
    [Required(ErrorMessage = "Required")]
    public int StatusId { get; set; }
    [BindNever]
    public IEnumerable<StatusModel>? Statuses { get; set; }
}