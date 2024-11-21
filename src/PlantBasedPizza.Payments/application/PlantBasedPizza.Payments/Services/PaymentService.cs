using System.Security.Cryptography;
using Dapr.Client;
using Grpc.Core;
using PlantBasedPizza.Payments.IntegrationEvents;

namespace PlantBasedPizza.Payments.Services;

public class PaymentService : Payment.PaymentBase
{
    private readonly DaprClient _daprClient;

    public PaymentService(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public override async Task<TakePaymentsReply> TakePayment(TakePaymentRequest request, ServerCallContext context)
    {
        var randomSecondDelay = RandomNumberGenerator.GetInt32(1, 250);

        await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));

        var evt = new PaymentSuccessfulEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            CustomerIdentifier = request.CustomerIdentifier
        };
        
        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);

        return new TakePaymentsReply()
        {
            IsSuccess = true,
            PaymentStatus = "SUCCESS"
        };
    }
}