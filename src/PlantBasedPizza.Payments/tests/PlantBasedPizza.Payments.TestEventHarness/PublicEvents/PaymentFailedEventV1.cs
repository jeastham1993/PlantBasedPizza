using System.Text.Json.Serialization;

namespace PlantBasedPizza.Payments.TestEventHarness.PublicEvents;

public class PaymentFailedEventV1
{
    [JsonIgnore]
    public const string EVENT_NAME = "payments.paymentFailed";
    
    public string? OrderIdentifier { get; init; }
}