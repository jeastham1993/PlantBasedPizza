using Dapr.Client;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.PublicEvents;
using Saunter.Attributes;

namespace PlantBasedPizza.Kitchen.Infrastructure;

[AsyncApi]
public class KitchenEventPublisher : IKitchenEventPublisher
{
    private readonly DaprClient _daprClient;
    private const string PUB_SUB_NAME = "public";

    public KitchenEventPublisher(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    [Channel("kitchen.orderConfirmed.v1")]
    [PublishOperation(typeof(KitchenConfirmedOrderEventV1), Summary = "Published when the kitchen confirms an order.")]
    public async Task PublishKitchenConfirmedOrderEventV1(KitchenConfirmedOrderEventV1 evt)
    {
        await _daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("kitchen.orderBaked.v1")]
    [PublishOperation(typeof(OrderBakedEventV1), Summary = "Published when the kitchen finishes baking an order.")]
    public async Task PublishOrderBakedEventV1(OrderBakedEventV1 evt)
    {
        await _daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("kitchen.orderPreparing.v1")]
    [PublishOperation(typeof(OrderPreparingEventV1), Summary = "Published when the kitchen starts preparing an order.")]
    public async Task PublishOrderPreparingEventV1(OrderPreparingEventV1 evt)
    {
        await _daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("kitchen.orderPrepComplete.v1")]
    [PublishOperation(typeof(OrderPreparingEventV1), Summary = "Published when the kitchen finishes preparing an order.")]
    public async Task PublishOrderPrepCompleteEventV1(OrderPrepCompleteEventV1 evt)
    {
        await _daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt);
    }

    [Channel("kitchen.qualityChecked.v1")]
    [PublishOperation(typeof(OrderQualityCheckedEventV1), Summary = "Published when the kitchen quality checks an order, this indicates it is ready.")]
    public async Task PublishOrderQualityCheckedEventV1(OrderQualityCheckedEventV1 evt)
    {
        await _daprClient.PublishEventAsync(PUB_SUB_NAME, $"{evt.EventName}.{evt.EventVersion}", evt);
    }
}