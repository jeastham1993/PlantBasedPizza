using System.Security.Cryptography;
using Dapr.Client;
using Grpc.Core;
using PlantBasedPizza.Payments.IntegrationEvents;

namespace PlantBasedPizza.Payments.Services;

public class PaymentService : Payment.PaymentBase
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(DaprClient daprClient, ILogger<PaymentService> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<TakePaymentsReply> TakePayment(TakePaymentRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received request to take payment for order {orderIdentifier}", request.OrderIdentifier);
        
        var randomSecondDelay = RandomNumberGenerator.GetInt32(1, 250);

        await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));
        
        this._logger.LogInformation("Publishing Payment Success Event");

        var evt = new PaymentSuccessfulEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            CustomerIdentifier = request.CustomerIdentifier
        };

        try
        {
            await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure publishing payment completed event");
        }
        
        _logger.LogInformation("Payment Success Event Published, returning success");

        return new TakePaymentsReply()
        {
            IsSuccess = true,
            PaymentStatus = "SUCCESS"
        };
    }
}