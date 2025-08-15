// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Core.CompleteOrder;

public class CompleteOrderCommandHandler(IOrderRepository orderRepository, IDomainEventDispatcher eventDispatcher)
{
    public async Task<OrderDto?> Handle(CompleteOrderCommand request)
    {
        try
        {
            var order = await orderRepository.Retrieve(request.OrderIdentifier);
            
            ArgumentNullException.ThrowIfNull(order);

            order.MarkAsCompleted();
            order.AddHistory("Order completed.");

            var evt = new OrderCompletedEvent(order.CustomerIdentifier, order.OrderIdentifier, order.TotalPrice)
            {
                CorrelationId = request.CorrelationId
            };

            await eventDispatcher.PublishAsync(evt);
            order.AddIntegrationEvent(evt);

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}