using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Core.Services;

public class DeliveryDomainService : IDeliveryDomainService
{
    private readonly IDomainEventDispatcher _eventDispatcher;

    public DeliveryDomainService(IDomainEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    public async Task ClaimDeliveryAsync(DeliveryRequest request, string driverName, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(request);

        request.AssignDriver(driverName);

        await _eventDispatcher.PublishAsync(new DriverCollectedOrderEvent(request.OrderIdentifier, driverName)
        {
            CorrelationId = correlationId
        });
    }

    public async Task CompleteDeliveryAsync(DeliveryRequest request, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(request);

        request.MarkAsDelivered();

        await _eventDispatcher.PublishAsync(new OrderDeliveredEvent(request.OrderIdentifier)
        {
            CorrelationId = correlationId
        });
    }
}