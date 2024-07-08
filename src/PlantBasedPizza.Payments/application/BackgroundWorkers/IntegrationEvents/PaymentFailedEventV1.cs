using PlantBasedPizza.Events;

namespace BackgroundWorkers.IntegrationEvents;

public class PaymentFailedEventV1 : IntegrationEvent
{
    public override string EventName => "payments.paymentFailed";
    public override string EventVersion => "v1";
    
    public override Uri Source => new("https://payments.plantbasedpizza");
    
    public string CustomerIdentifier { get; init; }
    public string OrderIdentifier { get; init; }
}