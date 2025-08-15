using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Core.Services;

public class DeliveryRequestFactory : IDeliveryRequestFactory
{
    private readonly IDomainEventDispatcher _eventDispatcher;

    public DeliveryRequestFactory(IDomainEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    public Task<DeliveryRequest> CreateAsync(string orderIdentifier, Address address, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(orderIdentifier);
        ArgumentNullException.ThrowIfNull(address);

        var deliveryRequest = new DeliveryRequest(orderIdentifier, address);

        // Note: No creation event needed based on current domain requirements
        // Delivery requests are created in response to OrderReadyForDeliveryEvent

        return Task.FromResult(deliveryRequest);
    }
}