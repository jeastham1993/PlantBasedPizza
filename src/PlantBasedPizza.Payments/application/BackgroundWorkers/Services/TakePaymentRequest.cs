namespace BackgroundWorkers.Services;

public class TakePaymentRequest
{
    public string OrderIdentifier { get; set; }
    
    public string CustomerIdentifier { get; set; }
    
    public decimal PaymentAmount { get; set; }
}