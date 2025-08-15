namespace PlantBasedPizza.Payment.Core;

public class TakePaymentCommand
{
    public string OrderIdentifier { get; set; }
    
    public decimal Amount { get; set; }
}