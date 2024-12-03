using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using Temporalio.Common;
using Temporalio.Workflows;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public enum OrderStatus
{
    Pending,
    Cancelled,
    Confirmed,
    Completed
}

[Workflow]
public class OrderProcessingWorkflow : IOrderWorkflow
{
    private OrderDto? _currentOrder;
    private OrderStatus _currentStatus = OrderStatus.Pending;
    private bool _orderPaidFor;
    private decimal _paymentAmount;
    private bool _orderCancelled;
    private bool _kitchenCompletedOrder;
    private bool _orderCollected;
    private bool _orderDelivered;

    [WorkflowRun]
    public async Task<OrderStatus> RunAsync(OrderDto? order)
    {
        _currentStatus = OrderStatus.Pending;
        _currentOrder = order;

        await WaitForPotentialCancellation();

        if (_orderCancelled)
        {
            await ProcessCancellation();
            return _currentStatus;
        }

        await SubmitOrder();

        await TakePayment();

        if (_orderCancelled)
        {
            await ProcessCancellation();
            return _currentStatus;
        }

        await ConfirmOrder();

        _currentStatus = OrderStatus.Confirmed;

        await WaitForKitchen();

        if (order.OrderType == OrderType.Delivery)
            await WaitForDelivery();
        else
            await WaitForCollection();
        
        _currentStatus = OrderStatus.Completed;

        return _currentStatus;
    }

    [WorkflowSignal]
    public async Task ReceivePaymentFor(decimal paymentAmount)
    {
        _orderPaidFor = true;
        _paymentAmount = paymentAmount;
    }

    [WorkflowSignal]
    public async Task CancelOrder()
    {
        _orderCancelled = true;
        await Workflow.ExecuteActivityAsync(
            (OrderActivities act) => act.CancelOrder(_currentOrder!.OrderIdentifier),
            new ActivityOptions
            {
                ScheduleToCloseTimeout = TimeSpan.FromSeconds(30), RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3,
                    BackoffCoefficient = 2
                }
            });
    }

    [WorkflowSignal]
    public async Task OrderReadyForDelivery()
    {
        _kitchenCompletedOrder = true;
        await Workflow.ExecuteActivityAsync(
            (OrderActivities act) => act.OrderReadyForDelivery(_currentOrder!.OrderIdentifier),
            new ActivityOptions
            {
                ScheduleToCloseTimeout = TimeSpan.FromSeconds(30), RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3,
                    BackoffCoefficient = 2
                }
            });
    }

    [WorkflowSignal]
    public async Task OrderDelivered()
    {
        _orderDelivered = true;
        await Workflow.ExecuteActivityAsync(
            (OrderActivities act) => act.OrderDelivered(_currentOrder!.OrderIdentifier),
            new ActivityOptions
            {
                ScheduleToCloseTimeout = TimeSpan.FromSeconds(30), RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3,
                    BackoffCoefficient = 2
                }
            });
    }

    [WorkflowSignal]
    public async Task OrderCollected()
    {
        _orderCollected = true;
        await Workflow.ExecuteActivityAsync(
            (OrderActivities act) => act.OrderCollected(_currentOrder!.OrderIdentifier),
            new ActivityOptions
            {
                ScheduleToCloseTimeout = TimeSpan.FromSeconds(30), RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3,
                    BackoffCoefficient = 2
                }
            });
    }

    public async Task WaitForPotentialCancellation()
    {
        await Workflow.DelayAsync(TimeSpan.FromSeconds(10));
    }

    public async Task WaitForKitchen()
    {
        while (!_kitchenCompletedOrder) await Workflow.DelayAsync(TimeSpan.FromSeconds(30));
    }

    public async Task WaitForCollection()
    {
        while (!_orderCollected) await Workflow.DelayAsync(TimeSpan.FromSeconds(30));
    }

    public async Task WaitForDelivery()
    {
        while (!_orderDelivered) await Workflow.DelayAsync(TimeSpan.FromSeconds(30));
    }

    public async Task SubmitOrder()
    {
        await Workflow.ExecuteActivityAsync(
            (OrderActivities act) => act.SubmitOrder(_currentOrder!),
            new ActivityOptions
            {
                ScheduleToCloseTimeout = TimeSpan.FromMinutes(2), RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3,
                    BackoffCoefficient = 2
                }
            });
    }

    public async Task TakePayment()
    {
        await Workflow.ExecuteActivityAsync(
            (OrderActivities act) => act.TakePayment(_currentOrder!),
            new ActivityOptions
            {
                ScheduleToCloseTimeout = TimeSpan.FromMinutes(2), RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3,
                    BackoffCoefficient = 2
                }
            });

        while (!_orderPaidFor) await Workflow.DelayAsync(TimeSpan.FromSeconds(2));
    }

    public async Task ConfirmOrder()
    {
        await Workflow.ExecuteActivityAsync(
            (OrderActivities act) => act.ConfirmOrder(_currentOrder!.OrderIdentifier, _paymentAmount),
            new ActivityOptions
            {
                ScheduleToCloseTimeout = TimeSpan.FromSeconds(10), RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3,
                    BackoffCoefficient = 2
                }
            });
    }

    public async Task ProcessCancellation()
    {
        _currentStatus = OrderStatus.Cancelled;

        if (_orderPaidFor)
            await Workflow.ExecuteActivityAsync(
                (OrderActivities act) => act.RefundPayment(_currentOrder!),
                new ActivityOptions
                {
                    ScheduleToCloseTimeout = TimeSpan.FromMinutes(2), RetryPolicy = new RetryPolicy
                    {
                        MaximumAttempts = 3,
                        BackoffCoefficient = 2
                    }
                });
    }
}