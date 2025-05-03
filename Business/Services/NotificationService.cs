using Business.Hubs;
using Data.Contexts;
using Data.Enteties.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;

public interface INotificationService
{
    Task AddNotificationAsync(NotificationEntity notificationEntity, string userId = "anonymous");
    Task DismissNotificationAsync(string notificaionId, string userId);
    Task<IEnumerable<NotificationEntity>> GetNotificationsAsync(string userId, int take = 10);
}

public class NotificationService(DataContext context, IHubContext<NotificationHub> notificationHub) : INotificationService
{
    private readonly DataContext _context = context;
    private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;

    public async Task AddNotificationAsync(NotificationEntity notificationEntity, string userId = "anonymous")
    {
        if (string.IsNullOrEmpty(notificationEntity.Image))
        {
            switch (notificationEntity.NotificationTypeId)
            {
                case 1:
                    notificationEntity.Image = "/Images/templates/user-template.svg";
                    break;

                case 2:
                    notificationEntity.Image = "/Images/templates/project-template.svg";
                    break;
            }
        }

        _context.Add(notificationEntity);
        await _context.SaveChangesAsync();

        var notifications = await GetNotificationsAsync(userId);
        var newNotification = notifications.OrderByDescending(x => x.Created).FirstOrDefault();

        if (newNotification != null)
        {
            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", newNotification);
        }
    }

    public async Task<IEnumerable<NotificationEntity>> GetNotificationsAsync(string userId, int take = 10)
    {
        var dismissedIds = await _context.DismissedNotifications
            .Where(x => x.UserId == userId)
            .Select(x => x.NotificationId)
            .ToListAsync();

        var notifications = await _context.Notifications
            .Where(x => !dismissedIds.Contains(x.Id))
            .OrderByDescending(x => x.Created)
            .Take(take)
            .ToListAsync();

        return notifications;
    }

    public async Task DismissNotificationAsync(string notificaionId, string userId)
    {
        var alreadyDismissed = await _context.DismissedNotifications.AnyAsync(x => x.NotificationId == notificaionId && x.UserId == userId);
        if (!alreadyDismissed)
        {
            var dismissed = new NotificationDismissedEntity
            {
                NotificationId = notificaionId,
                UserId = userId
            };

            _context.Add(dismissed);
            await _context.SaveChangesAsync();
        }
    }
}