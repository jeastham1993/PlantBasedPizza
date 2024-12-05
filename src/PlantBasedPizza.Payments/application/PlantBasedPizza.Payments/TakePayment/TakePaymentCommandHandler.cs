using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Payments.PublicEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.Payments.TakePayment;

public class TakePaymentCommandHandler(ILogger<TakePaymentCommandHandler> logger, IPaymentEventPublisher eventPublisher, IDistributedCache cache)
{
    [Channel("payments.takepayment.v1")]
    [PublishOperation(typeof(TakePaymentCommand), OperationId = nameof(TakePaymentCommand))]
    public async Task<bool> Handle(TakePaymentCommand command)
    {
        var hasOrderBeenProcessed = await cache.GetStringAsync(command.OrderIdentifier);

        if ((hasOrderBeenProcessed ?? "").Equals("processed"))
        {
            Activity.Current?.AddTag("payment-processed", "true");
            return true;
        }

        try
        {
            var randomSecondDelay = RandomNumberGenerator.GetInt32(1, 250);

            await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));
        
            logger.LogInformation("Publishing Payment Success Event");

            var successEvent = new PaymentSuccessfulEventV1()
            {
                OrderIdentifier = command.OrderIdentifier,
                Amount = Convert.ToDecimal(command.PaymentAmount)
            };
            
            await eventPublisher.PublishPaymentSuccessfulEventV1(successEvent);
            
            await cache.SetStringAsync(command.OrderIdentifier, "processed");

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure processing payment for order {OrderIdentifier}", command.OrderIdentifier);
            Activity.Current?.AddException(ex);
            
            await eventPublisher.PublishPaymentFailedEventV1(new PaymentFailedEventV1()
            {
                OrderIdentifier = command.OrderIdentifier
            });

            return false;
        }
    }
}