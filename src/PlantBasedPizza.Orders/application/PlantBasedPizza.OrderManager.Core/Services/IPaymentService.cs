namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IPaymentService
{
    Task TakePayment(string orderIdentifier, decimal paymentAmount);
    
    Task RefundPayment(string orderIdentifier, decimal paymentAmount);
}