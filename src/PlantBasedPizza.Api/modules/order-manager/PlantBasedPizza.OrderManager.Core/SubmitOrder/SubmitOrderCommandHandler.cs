// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Core.SubmitOrder;

public class SubmitOrderCommandHandler(IOrderRepository orderRepository, IPaymentService paymentService, IDomainEventDispatcher eventDispatcher)
{
    public async Task<OrderDto?> Handle(SubmitOrderCommand request)
    {
        try
        {
            var order = await orderRepository.Retrieve(request.OrderIdentifier);
            
            var takePayment = await paymentService.TakePaymentFor(order);

            if (string.IsNullOrEmpty(takePayment.PaymentId))
            {
                return null;
            }

            ArgumentNullException.ThrowIfNull(order);

            if (!order.Items.Any()) 
                throw new ArgumentException("Cannot submit an order with no items");

            order.MarkAsSubmitted();
            order.AddHistory("Submitted order.");

            await eventDispatcher.PublishAsync(new OrderSubmittedEvent(order.OrderIdentifier)
            {
                CorrelationId = string.Empty
            });

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}