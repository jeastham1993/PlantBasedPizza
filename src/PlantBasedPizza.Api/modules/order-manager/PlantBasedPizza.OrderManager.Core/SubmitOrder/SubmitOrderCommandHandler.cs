// Unless explicitly stated otherwise all files in this repository are licensed under the Apache License Version 2.0.
// This product includes software developed at Datadog (https://www.datadoghq.com/).
// Copyright 2025 Datadog, Inc.

using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Core.SubmitOrder;

public class SubmitOrderCommandHandler(IOrderRepository orderRepository, IPaymentService paymentService, IOrderDomainService orderDomainService)
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

            await orderDomainService.SubmitOrderAsync(order);

            await orderRepository.Update(order);

            return new OrderDto(order);
        }
        catch (OrderNotFoundException)
        {
            return null;
        }
    }
}