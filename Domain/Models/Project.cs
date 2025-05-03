namespace Domain.Models;

public class Project
{
    public string Id { get; set; } = null!;
    public string? Image { get; set; }
    public string ProjectName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string TimeLeft
    {
        get
        {
            if (EndDate.HasValue)
            {
                int days = (EndDate.Value.Date - DateTime.Today).Days;

                if (days < 0)
                    return "0 days left";

                else if (days < 7)
                    return $"{days} day{(days == 1 ? "" : "s")} left";

                else
                {
                    int weeks = days / 7;
                    return $"{weeks} week{(weeks == 1 ? "" : "s")} left";
                }
            }

            return "-";
        }
    }
    public string TimeLeftClass
    {
        get
        {
            if (EndDate.HasValue)
            {
                int days = (EndDate.Value.Date - DateTime.Today).Days;

                if (days < 0 || days < 7)
                    return "red";
            }
            return "";
        }
    }
    public DateTime Created { get; set; }
    public decimal? Budget { get; set; }
    public string ClientId { get; set; } = null!;
    public ClientModel? Client { get; set; }
    public string UserId { get; set; } = null!;
    public UserModel? User { get; set; }
    public int StatusId { get; set; }
    public StatusModel? Status { get; set; }
}