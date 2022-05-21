namespace PlantBasedPizza.Kitchen.Core.Errors;

public class OrderExistsException : Exception
{
    public OrderExistsException(string orderIdentifier) : base($"Order {orderIdentifier} exists")
    {
        this.OrderIdentifier = orderIdentifier;
    }
    
    public string OrderIdentifier { get; private set; }
}