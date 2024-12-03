namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IOrderWorkflow
{
    Task WaitForPotentialCancellation();

    Task SubmitOrder();
    
    Task CancelOrder();

    Task TakePayment();

    Task ReceivePaymentFor(decimal paymentAmount);
}