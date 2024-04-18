using System.Security.Cryptography;
using Grpc.Core;
using PlantBasedPizza.Events;
using PlantBasedPizza.Payments.IntegrationEvents;

namespace PlantBasedPizza.Payments.Services;

public class PaymentService : Payment.PaymentBase
{
    private readonly IEventPublisher _eventPublisher;

    public PaymentService(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public override async Task<TakePaymentsReply> TakePayment(TakePaymentRequest request, ServerCallContext context)
    {
        var randomSecondDelay = RandomNumberGenerator.GetInt32(1, 250);

        await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));

        await this._eventPublisher.Publish(new PaymentSuccessfulEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            CustomerIdentifier = request.CustomerIdentifier
        });

        return new TakePaymentsReply()
        {
            IsSuccess = true,
            PaymentStatus = "SUCCESS"
        };
    }
}