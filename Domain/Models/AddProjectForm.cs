namespace Domain.Models;

public class AddProjectForm
{
    public string? Image { get; set; }
    public string ProjectName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public int StatusId { get; set; }
    public string ClientId { get; set; } = null!;
    public string UserId { get; set; } = null!;
}