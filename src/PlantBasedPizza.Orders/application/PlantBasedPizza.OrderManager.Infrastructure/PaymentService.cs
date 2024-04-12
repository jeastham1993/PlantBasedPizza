using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class PaymentService : IPaymentService
{
    private readonly Payment.PaymentClient _paymentClient;

    public PaymentService(Payment.PaymentClient paymentClient)
    {
        _paymentClient = paymentClient;
    }

    public async Task<TakePaymentResult> TakePaymentFor(Order order)
    {
        var result =
            await this._paymentClient.TakePaymentAsync(new TakePaymentRequest()
            {
                CustomerIdentifier = order.CustomerIdentifier,
                PaymentAmount = Convert.ToDouble(order.TotalPrice)
            });

        return new TakePaymentResult(result.PaymentStatus, result.IsSuccess);
    }
}