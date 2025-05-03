using System.ComponentModel.DataAnnotations;

namespace Data.Enteties.Notifications;

public class NotificationTypeEntity
{
    [Key]
    public int Id { get; set; }
    public string NotificationType { get; set; } = null!;
    public ICollection<NotificationEntity> Notifications { get; set; } = [];
}