using System.Security.Cryptography;
using BackgroundWorkers.IntegrationEvents;
using PlantBasedPizza.Events;

namespace BackgroundWorkers.Services;

public class PaymentService
{
    private readonly IEventPublisher _eventPublisher;

    public PaymentService(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task TakePayment(TakePaymentRequest request)
    {
        var randomSecondDelay = RandomNumberGenerator.GetInt32(1, 1500);

        await Task.Delay(TimeSpan.FromMilliseconds(randomSecondDelay));

        await this._eventPublisher.Publish(new PaymentSuccessfulEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            CustomerIdentifier = request.CustomerIdentifier
        });
    }
}