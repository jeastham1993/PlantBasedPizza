namespace PlantBasedPizza.Payments.TestEventHarness;

public class InMemoryEventMonitor
{
    private List<ReceivedEvent> events = new();
    
    public IEnumerable<ReceivedEvent> GetEvent(string entityId)
    {
        var receivedEvents = events.Where(item => item.EntityId == entityId);
        
        return receivedEvents;
    }

    public void Add(ReceivedEvent evt)
    {
        events.Add(evt);
    }
}