using Microsoft.AspNetCore.SignalR;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Orders.Worker.Notifications;

public class UserIdClaimUserProvider(ILogger<UserIdClaimUserProvider> logger): IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        logger.LogInformation("Getting user id from connection");
        
        var accountId = connection.User?.Claims.ExtractAccountId();
        return accountId;
    }
}