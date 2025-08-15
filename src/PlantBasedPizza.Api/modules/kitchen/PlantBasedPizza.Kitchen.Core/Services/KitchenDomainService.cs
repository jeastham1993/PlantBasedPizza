using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Kitchen.Core.Services;

public class KitchenDomainService : IKitchenDomainService
{
    private readonly IDomainEventDispatcher _eventDispatcher;

    public KitchenDomainService(IDomainEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    public async Task StartPreparingAsync(KitchenRequest request, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(request);

        request.StartPreparing();

        await _eventDispatcher.PublishAsync(new OrderPreparingEvent(request.OrderIdentifier)
        {
            CorrelationId = correlationId
        });
    }

    public async Task CompletePreparationAsync(KitchenRequest request, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(request);

        request.CompletePreparing();

        await _eventDispatcher.PublishAsync(new OrderPrepCompleteEvent(request.OrderIdentifier)
        {
            CorrelationId = correlationId
        });
    }

    public async Task CompleteBakingAsync(KitchenRequest request, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(request);

        request.CompleteBaking();

        await _eventDispatcher.PublishAsync(new OrderBakedEvent(request.OrderIdentifier)
        {
            CorrelationId = correlationId
        });
    }

    public async Task CompleteQualityCheckAsync(KitchenRequest request, string correlationId = "")
    {
        ArgumentNullException.ThrowIfNull(request);

        request.CompleteQualityCheck();

        await _eventDispatcher.PublishAsync(new OrderQualityCheckedEvent(request.OrderIdentifier)
        {
            CorrelationId = correlationId
        });
    }
}