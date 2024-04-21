namespace PlantBasedPizza.Kitchen.Core.Entities;

public interface IKitchenEventPublisher
{
    Task PublishKitchenConfirmedOrderEventV1(KitchenRequest request);

    Task PublishOrderBakedEventV1(KitchenRequest request);
    
    Task PublishOrderPreparingEventV1(KitchenRequest request);
    
    Task PublishOrderPrepCompleteEventV1(KitchenRequest request);
    
    Task PublishOrderQualityCheckedEventV1(KitchenRequest request);
}