using PlantBasedPizza.Deliver.Core.PublicEvents;

namespace PlantBasedPizza.Deliver.Core.Entities;

public interface IDeliveryEventPublisher
{
    Task PublishDriverOrderCollectedEventV1(DriverCollectedOrderEventV1 evt);
    
    Task PublishDriverDeliveredOrderEventV1(DriverDeliveredOrderEventV1 evt);
}