using Grpc.Core;
using Microsoft.Extensions.Configuration;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class PaymentService : IPaymentService
{
    private readonly Metadata _metadata;
    private readonly Payment.PaymentClient _paymentClient;

    public PaymentService(Payment.PaymentClient paymentClient, IConfiguration configuration)
    {
        _paymentClient = paymentClient;
        _metadata = new Metadata()
        {
            { "APIKey", configuration["Auth:PaymentApiKey"] }
        };
    }

    public async Task<TakePaymentResult> TakePaymentFor(Order order)
    {
        // Temporary implementation until inter-service comms in ECS are resolved.
        return new TakePaymentResult("OK", true);
        
        var result =
            await this._paymentClient.TakePaymentAsync(new TakePaymentRequest()
            {
                CustomerIdentifier = order.CustomerIdentifier,
                PaymentAmount = Convert.ToDouble(order.TotalPrice)
            }, _metadata);

        return new TakePaymentResult(result.PaymentStatus, result.IsSuccess);
    }
}