using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Enteties.Notifications;

public class NotificationEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(TargetGroup))]
    public int NotificationTargetGroupId { get; set; } = 1;
    public NotificationTargetGroupEntity TargetGroup { get; set; } = null!;

    [ForeignKey(nameof(NotificationType))]
    public int NotificationTypeId { get; set; }
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    public string? Image { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Created { get; set; } = DateTime.Now;

    public ICollection<NotificationDismissedEntity> DismissedNotifications { get; set; } = [];
}