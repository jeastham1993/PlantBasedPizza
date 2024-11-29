using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Orders.Worker.Notifications;

[Authorize(Roles = "user")]
public class OrderNotificationsHub(ILogger<OrderNotificationsHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected");
    }
}