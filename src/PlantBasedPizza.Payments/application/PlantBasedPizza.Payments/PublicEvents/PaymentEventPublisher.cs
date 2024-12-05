using Dapr.Client;
using PlantBasedPizza.Events;
using Saunter.Attributes;

namespace PlantBasedPizza.Payments.PublicEvents;

[AsyncApi]
public class PaymentEventPublisher(DaprClient daprClient) : IPaymentEventPublisher
{
    private const string SOURCE = "payments";
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

    [Channel(PaymentSuccessfulEventV1.EVENT_NAME)]
    [SubscribeOperation(typeof(PaymentSuccessfulEventV1), OperationId = nameof(PaymentSuccessfulEventV1), Summary = "Published when a payment is successfully taken.")]
    public async Task PublishPaymentSuccessfulEventV1(PaymentSuccessfulEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();
        
        evt.AddToTelemetry(eventId);
        
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType},
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DATE_FORMAT) },
        };
        
        await daprClient.PublishEventAsync("payments", eventType, evt, eventMetadata);
    }

    [Channel(PaymentFailedEventV1.EVENT_NAME)]
    [SubscribeOperation(typeof(PaymentFailedEventV1), OperationId = nameof(PaymentFailedEventV1), Summary = "Published when a payment fails.")]
    public async Task PublishPaymentFailedEventV1(PaymentFailedEventV1 evt)
    {
        var eventType = $"{evt.EventName}.{evt.EventVersion}";
        var eventId = Guid.NewGuid().ToString();
        
        evt.AddToTelemetry(eventId);
        
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType},
            { EventConstants.EVENT_ID_HEADER_KEY, eventId },
            { EventConstants.EVENT_TIME_HEADER_KEY, DateTime.UtcNow.ToString(DATE_FORMAT) },
        };
        
        await daprClient.PublishEventAsync("payments", eventType, evt, eventMetadata);
    }
}