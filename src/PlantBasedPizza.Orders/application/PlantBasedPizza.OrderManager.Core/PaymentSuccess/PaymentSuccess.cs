namespace PlantBasedPizza.OrderManager.Core.PaymentSuccess;

public record PaymentSuccess
{
    public string OrderIdentifier { get; }
    public decimal Amount { get; }

    public PaymentSuccess(string OrderIdentifier, decimal Amount)
    {
        if (string.IsNullOrEmpty(OrderIdentifier) || Amount <= 0)
        {
            throw new ArgumentException("OrderIdentifier and Amount must be provided.");
        }

        this.OrderIdentifier = OrderIdentifier;
        this.Amount = Amount;
    }   
}