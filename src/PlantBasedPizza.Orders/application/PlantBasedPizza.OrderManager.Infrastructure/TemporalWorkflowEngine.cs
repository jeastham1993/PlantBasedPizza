using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Temporalio.Client;
using Temporalio.Common;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class TemporalWorkflowEngine(ITemporalClient client, IOrderRepository orderRepository) : IWorkflowEngine
{
    public async Task StartOrderWorkflowFor(string orderIdentifier)
    {
        var order = await orderRepository.Retrieve(orderIdentifier);

        if (order is null) return;

        await client.StartWorkflowAsync(
            (OrderProcessingWorkflow wf) => wf.RunAsync(new OrderDto(order)),
            new WorkflowOptions(generateWorkflowIdFor(orderIdentifier), "orders-queue")
            {
                RetryPolicy = new RetryPolicy
                {
                    BackoffCoefficient = 2,
                    MaximumAttempts = 2,
                    NonRetryableErrorTypes = new List<string> { "Temporal.API.Error.AlreadyExists" }
                }
            });
    }

    public async Task ConfirmPayment(string orderIdentifier, decimal paymentAmount)
    {
        var handle = client.GetWorkflowHandle(generateWorkflowIdFor(orderIdentifier));
        await handle.SignalAsync<OrderProcessingWorkflow>(wf => wf.ReceivePaymentFor(paymentAmount));
    }

    public async Task CancelOrder(string orderIdentifier)
    {
        var handle = client.GetWorkflowHandle(generateWorkflowIdFor(orderIdentifier));
        await handle.SignalAsync<OrderProcessingWorkflow>(wf => wf.CancelOrder());
    }

    public async Task OrderReadyForDelivery(string orderIdentifier)
    {
        var handle = client.GetWorkflowHandle(generateWorkflowIdFor(orderIdentifier));
        await handle.SignalAsync<OrderProcessingWorkflow>(wf => wf.OrderReadyForDelivery());
    }

    public async Task OrderCollected(string orderIdentifier)
    {
        var handle = client.GetWorkflowHandle(generateWorkflowIdFor(orderIdentifier));
        await handle.SignalAsync<OrderProcessingWorkflow>(wf => wf.OrderCollected());
    }

    public async Task OrderDelivered(string orderIdentifier)
    {
        var handle = client.GetWorkflowHandle(generateWorkflowIdFor(orderIdentifier));
        await handle.SignalAsync<OrderProcessingWorkflow>(wf => wf.OrderDelivered());
    }

    private static string generateWorkflowIdFor(string orderIdentifier) => $"OrderProcessing_{orderIdentifier}";
}