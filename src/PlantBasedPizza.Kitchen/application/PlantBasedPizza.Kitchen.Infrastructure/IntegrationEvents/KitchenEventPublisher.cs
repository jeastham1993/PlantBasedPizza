using Dapr.Client;
using PlantBasedPizza.Kitchen.Core.Entities;
using Saunter.Attributes;

namespace PlantBasedPizza.Kitchen.Infrastructure.IntegrationEvents;

[AsyncApi]
public class KitchenEventPublisher : IKitchenEventPublisher
{
    private readonly DaprClient _daprClient;

    public KitchenEventPublisher(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    [Channel("kitchen.orderConfirmed.v1")]
    [PublishOperation(typeof(KitchenConfirmedOrderEventV1), Summary = "Published when the kitchen confirms an order.")]
    public async Task PublishKitchenConfirmedOrderEventV1(KitchenRequest request)
    {
        var evt = new KitchenConfirmedOrderEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            KitchenIdentifier = request.KitchenRequestId
        };

        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("kitchen.orderBaked.v1")]
    [PublishOperation(typeof(OrderBakedEventV1), Summary = "Published when the kitchen finishes baking an order.")]
    public async Task PublishOrderBakedEventV1(KitchenRequest request)
    {
        var evt = new OrderBakedEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            KitchenIdentifier = request.KitchenRequestId
        };
        
        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("kitchen.orderPreparing.v1")]
    [PublishOperation(typeof(OrderPreparingEventV1), Summary = "Published when the kitchen starts preparing an order.")]
    public async Task PublishOrderPreparingEventV1(KitchenRequest request)
    {
        var evt = new OrderPreparingEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            KitchenIdentifier = request.KitchenRequestId
        };
        
        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("kitchen.orderPrepComplete.v1")]
    [PublishOperation(typeof(OrderPreparingEventV1), Summary = "Published when the kitchen finishes preparing an order.")]
    public async Task PublishOrderPrepCompleteEventV1(KitchenRequest request)
    {
        var evt = new OrderPrepCompleteEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            KitchenIdentifier = request.KitchenRequestId
        };
        
        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("kitchen.qualityChecked.v1")]
    [PublishOperation(typeof(OrderQualityCheckedEventV1), Summary = "Published when the kitchen quality checks an order, this indicates it is ready.")]
    public async Task PublishOrderQualityCheckedEventV1(KitchenRequest request)
    {
        var evt = new OrderQualityCheckedEventV1()
        {
            OrderIdentifier = request.OrderIdentifier,
            KitchenIdentifier = request.KitchenRequestId
        };
        
        await _daprClient.PublishEventAsync("public", $"{evt.EventName}.{evt.EventVersion}", evt);
    }
}