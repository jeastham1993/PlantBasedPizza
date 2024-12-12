using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlantBasedPizza.Events;
using PlantBasedPizza.Payments.PublicEvents;

namespace PlantBasedPizza.Payments.TakePayment;

public class TakePaymentCommand : IntegrationEvent
{
    public override string EventName => "payments.takepayment";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("https://orders.plantbasedpizza");
    
    [JsonPropertyName("OrderIdentifier")]
    public string? OrderIdentifier { get; set; }
    
    [JsonPropertyName("TakePayment")]
    public decimal PaymentAmount { get; set; }

    public override string AsString()
    {
        return JsonSerializer.Serialize(this);
    }

    public VerificationResult Verify()
    {
        if (string.IsNullOrEmpty(OrderIdentifier))
        {
            Activity.Current?.AddTag("command.invalid", "true");
            Activity.Current?.AddTag("command.invalid_reason", "Order identifier is null or empty.");
            
            return new VerificationResult(false, false);
        }
        if (PaymentAmount <= 0)
        {
            Activity.Current?.AddTag("command.invalid", "true");
            Activity.Current?.AddTag("command.invalid_reason", "Payment amount is less than or equal to zero.");
            
            return new VerificationResult(false, true);
        }

        return new VerificationResult(true, false);
    }
}