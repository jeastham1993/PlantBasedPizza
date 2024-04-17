using System.Security.Cryptography;
using Grpc.Core;

namespace PlantBasedPizza.Payments.Services;

public class PaymentService : Payment.PaymentBase
{
    public override async Task<TakePaymentsReply> TakePayment(TakePaymentRequest request, ServerCallContext context)
    {
        var randomSecondDelay = RandomNumberGenerator.GetInt32(1, 250);

        await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));

        return new TakePaymentsReply()
        {
            IsSuccess = true,
            PaymentStatus = "SUCCESS"
        };
    }
}