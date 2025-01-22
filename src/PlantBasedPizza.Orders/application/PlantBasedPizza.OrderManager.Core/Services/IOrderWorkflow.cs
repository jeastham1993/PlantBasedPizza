namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IOrderWorkflow
{
    Task WaitForPotentialCancellation();

    Task SubmitOrder();
    
    Task CancelOrder(string cancellationReason);

    Task TakePayment();

    Task ReceivePaymentFor(decimal paymentAmount);
}