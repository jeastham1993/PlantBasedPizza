using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.Services;

public interface IDeliveryDomainService
{
    Task ClaimDeliveryAsync(DeliveryRequest request, string driverName, string correlationId = "");
    Task CompleteDeliveryAsync(DeliveryRequest request, string correlationId = "");
}