using PlantBasedPizza.Deliver.Core.Entities;

namespace PlantBasedPizza.Deliver.Core.Services;

public interface IDeliveryRequestFactory
{
    Task<DeliveryRequest> CreateAsync(string orderIdentifier, Address address, string correlationId = "");
}