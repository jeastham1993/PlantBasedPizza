using System.Text.Json.Serialization;

namespace PlantBasedPizza.Payments.TestEventHarness;

public record TakePaymentCommand
{
    [JsonPropertyName("OrderIdentifier")]
    public string? OrderIdentifier { get; set; }
    
    [JsonPropertyName("PaymentAmount")]
    public decimal PaymentAmount { get; set; }
}