using Data.Enteties.Notifications;
using Microsoft.AspNetCore.Identity;

namespace Data.Entities;

public class UserEntity : IdentityUser
{
    [ProtectedPersonalData]
    public string? FirstName { get; set; }
    [ProtectedPersonalData]
    public string? LastName { get; set; }
    [ProtectedPersonalData]
    public string? JobTitle { get; set; }
    public string? Image { get; set; }
    public MemberEntity? Address { get; set; }

    public ICollection<ProjectEntity> Projects { get; set; } = [];
    public ICollection<NotificationDismissedEntity> DismissedNotifications { get; set; } = [];
}