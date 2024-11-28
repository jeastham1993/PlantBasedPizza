using Dapr.Client;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.OrderSubmitted;
using PlantBasedPizza.OrderManager.Core.PublicEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.OrderManager.Infrastructure;

[AsyncApi]
public class OrderEventPublisher(DaprClient daprClient) : IOrderEventPublisher
{
    [Channel("order.orderSubmitted.v1")]
    [PublishOperation(typeof(OrderSubmittedEventV1), Summary = "Published when an order is submitted.")]
    public async Task PublishOrderSubmittedEventV1(OrderSubmittedEventV1 evt) => await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);

    [Channel("order.orderCompleted.v1")]
    [PublishOperation(typeof(OrderCompletedIntegrationEventV1), Summary = "Published when an order is completed.")]
    public async Task PublishOrderCompletedEventV1(OrderCompletedIntegrationEventV1 evt) => await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);

    [Channel("order.readyForDelivery.v1")]
    [PublishOperation(typeof(OrderReadyForDeliveryEventV1), Summary = "Published when a delivery order is ready for delivery.")]
    public async Task PublishOrderReadyForDeliveryEventV1(OrderReadyForDeliveryEventV1 evt) => await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);

    [Channel("order.orderConfirmed.v1")]
    [PublishOperation(typeof(OrderConfirmedEventV1), Summary = "Published when an order is fully confirmed.")]
    public async Task PublishOrderConfirmedEventV1(OrderConfirmedEventV1 evt) => await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);

    [Channel("order.orderCreated.v1")]
    [PublishOperation(typeof(OrderCreatedEventV1), Summary = "Published when an order is first created.")]
    public async Task PublishOrderCreatedEventV1(OrderCreatedEventV1 evt) => await daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
}