using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IWorkflowEngine
{
    Task StartOrderWorkflowFor(string orderIdentifier);

    Task ConfirmPayment(string orderIdentifier, decimal paymentAmount);

    Task CancelOrder(string orderIdentifier);

    Task OrderReadyForDelivery(string orderIdentifier);

    Task OrderCollected(string orderIdentifier);

    Task OrderDelivered(string orderIdentifier);
}