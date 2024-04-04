using System.Diagnostics;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints;

public static class ObservabilityExtensions
{
    public static void AddToTrace(this AddLoyaltyPointsCommand command)
    {
        Activity.Current?.AddTag("loyalty.customerId", command.CustomerIdentifier);
        Activity.Current?.AddTag("loyalty.orderIdentifier", command.OrderIdentifier);
    }
    
    public static void AddToTrace(this SpendLoyaltyPointsCommand command)
    {
        Activity.Current?.AddTag("loyalty.customerId", command.CustomerIdentifier);
        Activity.Current?.AddTag("loyalty.orderIdentifier", command.OrderIdentifier);
    }
}