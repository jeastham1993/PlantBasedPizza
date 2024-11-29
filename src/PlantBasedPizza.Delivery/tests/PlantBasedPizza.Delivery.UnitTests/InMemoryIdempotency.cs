using PlantBasedPizza.Delivery.Worker;

namespace PlantBasedPizza.Delivery.UnitTests;

public class InMemoryIdempotency : Idempotency
{
    private readonly HashSet<string> _knownEvents = new();
    
    public HashSet<string> HandledEvents => _knownEvents;
    
    public async Task<bool> HasEventBeenProcessedWithId(string eventId)
    {
        return _knownEvents.Contains(eventId);
    }

    public async Task ProcessedSuccessfully(string eventId)
    {
        _knownEvents.Add(eventId);
    }
}