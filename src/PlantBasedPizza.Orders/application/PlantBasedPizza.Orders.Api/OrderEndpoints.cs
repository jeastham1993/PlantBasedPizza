using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CancelOrder;
using PlantBasedPizza.OrderManager.Core.CollectOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.OrderManager.Core.SubmitOrder;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Orders.Api;

public static class OrderEndpoints
{
    public static async Task<IResult> GetForCustomer(HttpContext httpContext,
        [FromServices] IOrderRepository orderRepository)
    {
        try
        {
            var accountId = httpContext.User.Claims.ExtractAccountId();
            var orders = await orderRepository.ForCustomer(accountId);

            return Results.Ok(orders.Select(order => new OrderDto(order)));
        }
        catch (Exception e)
        {
            Activity.Current?.AddException(e);
            return Results.BadRequest(new List<OrderDto>());
        }
    }

    public static async Task<IResult> Get(HttpContext httpContext, [FromServices] IOrderRepository orderRepository,
        string orderIdentifier)
    {
        try
        {
            var accountId = httpContext.User.Claims.ExtractAccountId();
            var order = await orderRepository.Retrieve(orderIdentifier);

            if (order.CustomerIdentifier != accountId) throw new OrderNotFoundException(orderIdentifier);

            return Results.Ok(new OrderDto(order));
        }
        catch (OrderNotFoundException e)
        {
            Activity.Current?.AddException(e);
            return Results.NotFound();
        }
    }

    public static async Task<IResult> CreatePickupOrder(HttpContext httpContext, CreatePickupOrderCommand request,
        [FromServices] CreatePickupOrderCommandHandler handler)
    {
        try
        {
            request.CustomerIdentifier = httpContext.User.Claims.ExtractAccountId();
            var order = await handler.Handle(request);

            return Results.Created($"/order/{order?.OrderIdentifier}/detail", order);
        }
        catch (Exception e)
        {
            Activity.Current?.AddException(e);
            return Results.InternalServerError();
        }
    }

    public static async Task<IResult> CreateDeliveryOrder(HttpContext httpContext, CreateDeliveryOrder request,
        [FromServices] CreateDeliveryOrderCommandHandler handler)
    {
        try
        {
            request.CustomerIdentifier = httpContext.User.Claims.ExtractAccountId();
            var order = await handler.Handle(request);

            return Results.Created($"/order/{order?.OrderIdentifier}/detail", order);
        }
        catch (Exception e)
        {
            Activity.Current?.AddException(e);
            return Results.InternalServerError();
        }
    }

    public static async Task<IResult> AddItemToOrder(HttpContext httpContext, AddItemToOrderCommand request,
        [FromServices] AddItemToOrderHandler handler)
    {
        try
        {
            request.AddToTelemetry();
            request.CustomerIdentifier = httpContext.User.Claims.ExtractAccountId();

            var order = await handler.Handle(request);

            if (order is null) return Results.NotFound();

            return Results.Ok(new OrderDto(order));
        }
        catch (Exception e)
        {
            Activity.Current?.AddException(e);
            return Results.InternalServerError();
        }
    }

    public static async Task<IResult> SubmitOrder(HttpContext httpContext, string orderIdentifier,
        [FromServices] SubmitOrderCommandHandler handler,
        [FromServices] IFeatures features,
        [FromServices] IWorkflowEngine workflowEngine, 
        [FromServices] IPaymentService paymentService)
    {
        try
        {
            var accountId = httpContext.User.Claims.ExtractAccountId();
            
            if (features.UseOrchestrator())
            {
                await workflowEngine.StartOrderWorkflowFor(orderIdentifier);
                return Results.Created();
            }

            var result = await handler.Handle(new SubmitOrderCommand
            {
                OrderIdentifier = orderIdentifier,
                CustomerIdentifier = accountId
            });

            await paymentService.TakePayment(result.OrderIdentifier, result.TotalPrice);

            return Results.Created($"/order/{result?.OrderIdentifier}/detail", result);
        }
        catch (Exception e)
        {
            Activity.Current?.AddException(e);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> CancelOrder(HttpContext httpContext, string orderIdentifier,
        [FromServices] CancelOrderCommandHandler handler,
        [FromServices] IFeatures features,
        [FromServices] IWorkflowEngine workflowEngine,
        [FromBody] CancelOrderCommand command)
    {
        var accountId = httpContext.User.Claims.ExtractAccountId();

        try
        {
            if (features.UseOrchestrator())
            {
                await workflowEngine.CancelOrder(command.OrderIdentifier);
                return Results.Ok();
            }

            var result = await handler.Handle(command);

            if (result.CancelSuccess)
                return Results.Ok();

            return Results.BadRequest();
        }
        catch (Exception e)
        {
            Activity.Current?.AddException(e);
            return Results.StatusCode(500);
        }
    }

    public static async Task<IResult> GetAwaitingCollection(IOrderRepository orderRepository)
    {
        try
        {
            var awaitingCollection = await orderRepository.GetAwaitingCollection();
            return Results.Ok(awaitingCollection.Select(order => new OrderDto(order)));
        }
        catch (Exception e)
        {
            Activity.Current?.AddException(e);
            return Results.InternalServerError();
        }
    }

    public static async Task<IResult> OrderCollected(CollectOrderRequest request,
        [FromServices] CollectOrderCommandHandler handler,
        [FromServices] IFeatures features,
        [FromServices] IWorkflowEngine workflowEngine)
    {
        try
        {
            if (features.UseOrchestrator())
            {
                await workflowEngine.OrderCollected(request.OrderIdentifier);
                return Results.Ok();
            }
            
            var result = await handler.Handle(request);
            return result != null ? Results.Ok(result) : Results.NotFound();
        }
        catch (Exception e)
        {
            Activity.Current?.AddException(e);
            return Results.InternalServerError();
        }
    }
}