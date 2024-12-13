namespace PlantBasedPizza.Payments.TestEventHarness.PublicEvents;

public class PaymentSuccessfulEventV1
{
    public string? OrderIdentifier { get; init; }
    
    public decimal Amount { get; init; }
}