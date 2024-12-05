using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Payments.TakePayment;
using Saunter.Attributes;

namespace PlantBasedPizza.Payments.RefundPayment;

public class RefundPaymentCommandHandler(ILogger<RefundPaymentCommandHandler> logger, IDistributedCache cache)
{
    [Channel("payments.refundpayment.v1")]
    [PublishOperation(typeof(RefundPaymentCommand), OperationId = nameof(RefundPaymentCommand))]
    public async Task<bool> Handle(RefundPaymentCommand command)
    {
        var hasOrderBeenProcessed = await cache.GetStringAsync($"Refunded_{command.OrderIdentifier}");

        if ((hasOrderBeenProcessed ?? "").Equals("processed"))
        {
            Activity.Current?.AddTag("payment-refunded", "true");
            return true;
        }

        try
        {
            var randomSecondDelay = RandomNumberGenerator.GetInt32(5, 20);

            await Task.Delay(TimeSpan.FromSeconds(randomSecondDelay));
            
            await cache.SetStringAsync($"Refunded_{command.OrderIdentifier}", "processed");

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure refunding payment for order {OrderIdentifier}", command.OrderIdentifier);
            Activity.Current?.AddException(ex);

            return false;
        }
    }
}