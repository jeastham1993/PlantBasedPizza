using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.Core.CancelOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.ConfirmOrder;
using PlantBasedPizza.OrderManager.Core.ExternalEvents;
using PlantBasedPizza.OrderManager.Core.OrderDelivered;
using PlantBasedPizza.OrderManager.Core.OrderReadyForDelivery;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Core.SubmitOrder;
using Temporalio.Activities;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderActivities(SubmitOrderCommandHandler submitOrderHandler,
    ConfirmOrderCommandHandler confirmOrderHandler,
    CancelOrderCommandHandler cancelOrderHandler,
    OrderReadyForDeliveryCommandHandler orderReadyForDeliveryHandler,
    CollectOrderCommandHandler collectOrderCommandHandler,
    OrderDeliveredEventHandler orderDeliveredEventHandler,
    IPaymentService paymentService)
{
    [Activity]
    public async Task SubmitOrder(OrderDto order)
    {
        await submitOrderHandler.Handle(new SubmitOrderCommand()
        {
            OrderIdentifier = order.OrderIdentifier,
            CustomerIdentifier = order.CustomerIdentifier,
        });
    }
    
    [Activity]
    public async Task TakePayment(OrderDto order)
    {
        await paymentService.TakePayment(order.OrderIdentifier, order.TotalPrice);
    }
    
    [Activity]
    public async Task RefundPayment(OrderDto order)
    {
        await paymentService.RefundPayment(order.OrderIdentifier, order.TotalPrice);
    }

    [Activity]
    public async Task ConfirmOrder(string orderIdentifier, decimal paymentAmount)
    {
        await confirmOrderHandler.Handle(new ConfirmOrderCommand()
        {
            OrderIdentifier = orderIdentifier,
            PaymentAmount = paymentAmount,
        });
    }

    [Activity]
    public async Task CancelOrder(string orderIdentifier, string cancellationReason)
    {
        await cancelOrderHandler.Handle(new CancelOrderCommand()
        {
            OrderIdentifier = orderIdentifier,
            CancellationReason = cancellationReason
        });
    }

    [Activity]
    public async Task OrderReadyForDelivery(string orderIdentifier)
    {
        await orderReadyForDeliveryHandler.Handle(new OrderReadyForDeliveryCommand()
        {
            OrderIdentifier = orderIdentifier
        });
    }

    [Activity]
    public async Task OrderCollected(string orderIdentifier)
    {
        await collectOrderCommandHandler.Handle(new CollectOrderRequest()
        {
            OrderIdentifier = orderIdentifier
        });
    }

    [Activity]
    public async Task OrderDelivered(string orderIdentifier)
    {
        await orderDeliveredEventHandler.Handle(new OrderDelivered(orderIdentifier));
    }
}