using System.Text.Json;
using System.Text.Json.Serialization;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Payments.TakePayment;

public class TakePaymentCommand : IntegrationEvent
{
    public override string EventName => "payments.takepayment";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.plantbasedpizza");
    
    [JsonPropertyName("OrderIdentifier")]
    public string OrderIdentifier { get; set; }
    
    [JsonPropertyName("TakePayment")]
    public decimal PaymentAmount { get; set; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}