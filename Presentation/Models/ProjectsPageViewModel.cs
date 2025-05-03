using Domain.Models;

namespace WebApp.Models;

public class ProjectsPageViewModel
{
    public IEnumerable<Project> Projects { get; set; } = null!;
    public IEnumerable<Project> FilteredProjects { get; set; } = null!;
    public IEnumerable<ClientModel> Clients { get; set; } = null!;
    public IEnumerable<UserModel> Users { get; set; } = null!;
    public IEnumerable<StatusModel> Statuses { get; set; } = null!;
    public AddProjectModalViewModel AddProject { get; set; } = new();
    public EditProjectModalViewModel EditProject { get; set; } = new();
}