using Dapr.Client;
using Saunter.Attributes;

namespace PlantBasedPizza.Payments.PublicEvents;

[AsyncApi]
public class PaymentEventPublisher(DaprClient daprClient) : IPaymentEventPublisher
{
    private const string SOURCE = "payments";
    
    [Channel(PaymentSuccessfulEventV1.EVENT_NAME)]
    [PublishOperation(typeof(PaymentSuccessfulEventV1), Summary = "Published when an order is submitted.")]
    public async Task PublishPaymentSuccessfulEventV1(PaymentSuccessfulEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { "cloudevent.source", SOURCE },
            { "cloudevent.type", $"{evt.EventName}.{evt.EventVersion}" },
            { "cloudevent.id", Guid.NewGuid().ToString() }
        };
            
        await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }

    [Channel(PaymentFailedEventV1.EVENT_NAME)]
    [PublishOperation(typeof(PaymentFailedEventV1), Summary = "Published when an order is submitted.")]
    public async Task PublishPaymentFailedEventV1(PaymentFailedEventV1 evt)
    {
        var eventMetadata = new Dictionary<string, string>(2)
        {
            { "cloudevent.source", SOURCE },
            { "cloudevent.type", $"{evt.EventName}.{evt.EventVersion}" },
            { "cloudevent.id", Guid.NewGuid().ToString() }
        };
            
        await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt, eventMetadata);
    }
}