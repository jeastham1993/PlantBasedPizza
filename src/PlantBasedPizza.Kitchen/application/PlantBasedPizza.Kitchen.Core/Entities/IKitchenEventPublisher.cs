using PlantBasedPizza.Kitchen.Core.PublicEvents;

namespace PlantBasedPizza.Kitchen.Core.Entities;

public interface IKitchenEventPublisher
{
    Task PublishKitchenConfirmedOrderEventV1(KitchenConfirmedOrderEventV1 evt);

    Task PublishOrderBakedEventV1(OrderBakedEventV1 evt);
    
    Task PublishOrderPreparingEventV1(OrderPreparingEventV1 evt);
    
    Task PublishOrderPrepCompleteEventV1(OrderPrepCompleteEventV1 evt);
    
    Task PublishOrderQualityCheckedEventV1(OrderQualityCheckedEventV1 evt);
}