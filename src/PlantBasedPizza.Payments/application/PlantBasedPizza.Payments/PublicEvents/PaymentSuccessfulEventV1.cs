using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Payments.PublicEvents;

public class PaymentSuccessfulEventV1 : IntegrationEvent
{
    [JsonIgnore]
    public const string EVENT_NAME = "payments.paymentSuccessful";
    
    [JsonIgnore]
    public override string EventName => EVENT_NAME;
    
    [JsonIgnore]
    public override string EventVersion => "v1";
    
    [JsonIgnore]
    public override Uri Source => new("https://payments.plantbasedpizza.com");
    
    public string OrderIdentifier { get; init; }
    
    public decimal Amount { get; init; }
}