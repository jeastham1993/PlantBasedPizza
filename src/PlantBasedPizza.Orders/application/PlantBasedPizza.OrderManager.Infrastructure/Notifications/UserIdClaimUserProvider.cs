using Microsoft.AspNetCore.SignalR;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Infrastructure.Notifications;

public class UserIdClaimUserProvider: IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var accountId = connection.User?.Claims.ExtractAccountId();
        return accountId;
    }
}