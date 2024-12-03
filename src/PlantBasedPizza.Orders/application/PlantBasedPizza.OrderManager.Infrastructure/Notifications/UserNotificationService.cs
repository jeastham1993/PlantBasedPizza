using Microsoft.AspNetCore.SignalR;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure.Notifications;

public class UserNotificationService(IHubContext<OrderNotificationsHub> hub) : IUserNotificationService
{
    public async Task NotifyPaymentSuccess(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("paymentSuccess", orderIdentifier);
    }

    public async Task NotifyOrderPreparing(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("preparing", orderIdentifier);
    }

    public async Task NotifyOrderPrepComplete(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("prepComplete", orderIdentifier);
    }

    public async Task NotifyOrderBakeComplete(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("bakeComplete", orderIdentifier);
    }

    public async Task NotifyOrderQualityCheckComplete(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("qualityCheckComplete", orderIdentifier);
    }

    public async Task NotifyOrderDriverAssigned(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("driverAssigned", orderIdentifier);
    }

    public async Task NotifyReadyForCollection(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("readyForCollection", orderIdentifier);
    }

    public async Task NotifyOrderCancelled(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("orderCancelled", orderIdentifier);
    }

    public async Task NotifyCancellationFailed(string customerIdentifier, string orderIdentifier)
    {
        await hub.Clients.User(customerIdentifier).SendAsync("cancellationFailed", orderIdentifier);
    }
}