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
            { "APIKey", configuration["Auth:PaymentApiKey"] },
            { "dapr-app-id", "payment" }
        };
    }

    public async Task<TakePaymentResult> TakePaymentFor(Order order)
    {
        var result =
            await _paymentClient.TakePaymentAsync(new TakePaymentRequest()
            {
                CustomerIdentifier = order.CustomerIdentifier,
                PaymentAmount = Convert.ToDouble(order.TotalPrice)
            }, _metadata);

        return new TakePaymentResult(result.PaymentStatus, result.IsSuccess);
    }
}