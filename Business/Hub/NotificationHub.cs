using Microsoft.AspNetCore.SignalR;

namespace Business.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(object notification)
    {
        await Clients.All.SendAsync("ReceiveNotification", notification);
    }
}