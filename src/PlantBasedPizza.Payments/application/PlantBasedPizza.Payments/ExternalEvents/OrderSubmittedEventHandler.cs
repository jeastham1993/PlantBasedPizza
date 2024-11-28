using System.Diagnostics;
using System.Security.Cryptography;
using Dapr.Client;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Orders.Internal;
using PlantBasedPizza.Payments.IntegrationEvents;

namespace PlantBasedPizza.Payments.ExternalEvents;

public class OrderSubmittedEventHandler(ILogger<OrderSubmittedEventHandler> logger, Orders.Internal.Orders.OrdersClient orderClient, DaprClient daprClient, IDistributedCache cache)
{
    private Metadata metadata = new()
    {
        { "dapr-app-id", "orders-internal" }
    };
    
    public async Task Handle(OrderSubmittedEventV1 evt)
    {
        logger.LogInformation("Handling order submitted event for {orderIdentifier}", evt.OrderIdentifier);
        
        var hasOrderBeenProcessed = await cache.GetStringAsync(evt.OrderIdentifier);

        if ((hasOrderBeenProcessed ?? "").Equals("processed"))
        {
            Activity.Current?.AddTag("payment-processed", "true");
            return;
        }
        
        var order = await orderClient.GetOrderDetailsAsync(new GetOrderDetailsRequest()
        {
            OrderIdentifier = evt.OrderIdentifier
        }, metadata);
        
        var randomSecondDelay = RandomNumberGenerator.GetInt32(1, 250);

        await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));
        
        logger.LogInformation("Publishing Payment Success Event");

        var successEvent = new PaymentSuccessfulEventV1()
        {
            OrderIdentifier = order.OrderIdentifier,
            Amount = Convert.ToDecimal(order.OrderValue)
        };

        try
        {
            await daprClient.PublishEventAsync("public", $"{successEvent.EventName}.{successEvent.EventVersion}", successEvent);
            
            await cache.SetStringAsync(evt.OrderIdentifier, "processed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure publishing payment completed event");
        }
    }
}