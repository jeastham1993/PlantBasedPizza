using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Payments.ExternalEvents;
using PlantBasedPizza.Payments.PublicEvents;

namespace PlantBasedPizza.Payments.OrderSubmitted;

public class OrderSubmittedEventHandler(ILogger<OrderSubmittedEventHandler> logger, IPaymentEventPublisher eventPublisher, IOrderService orderService, IDistributedCache cache)
{
    
    public async Task<bool> Handle(OrderSubmittedEventV1 evt)
    {
        var hasOrderBeenProcessed = await cache.GetStringAsync(evt.OrderIdentifier);

        if ((hasOrderBeenProcessed ?? "").Equals("processed"))
        {
            Activity.Current?.AddTag("payment-processed", "true");
            return true;
        }

        try
        {
            var order = await orderService.GetOrderDetails(evt.OrderIdentifier);
        
            var randomSecondDelay = RandomNumberGenerator.GetInt32(1, 250);

            await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));
        
            logger.LogInformation("Publishing Payment Success Event");

            var successEvent = new PaymentSuccessfulEventV1()
            {
                OrderIdentifier = order.OrderIdentifier,
                Amount = Convert.ToDecimal(order.OrderValue)
            };
            
            await eventPublisher.PublishPaymentSuccessfulEventV1(successEvent);
            
            await cache.SetStringAsync(evt.OrderIdentifier, "processed");

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure processing payment for order {OrderIdentifier}", evt.OrderIdentifier);
            Activity.Current?.AddException(ex);
            
            await eventPublisher.PublishPaymentFailedEventV1(new PaymentFailedEventV1()
            {
                OrderIdentifier = evt.OrderIdentifier
            });

            return false;
        }
    }
}