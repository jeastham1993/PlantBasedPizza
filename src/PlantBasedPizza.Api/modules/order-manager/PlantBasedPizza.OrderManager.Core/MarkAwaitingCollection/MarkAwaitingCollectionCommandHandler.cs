// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.MarkAwaitingCollection;

public class MarkAwaitingCollectionCommandHandler(IOrderRepository orderRepository)
{
    public async Task<OrderDto?> Handle(MarkAwaitingCollectionCommand request)
    {
        try
        {
            var order = await orderRepository.Retrieve(request.OrderIdentifier);
            
            ArgumentNullException.ThrowIfNull(order);

            order.MarkAsAwaitingCollection();
            order.AddHistory("Order awaiting collection");

            // Note: No event needed for this operation based on current domain logic

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}