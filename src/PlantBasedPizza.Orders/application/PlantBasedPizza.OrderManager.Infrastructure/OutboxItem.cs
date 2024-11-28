namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OutboxItem
{
    public OutboxItem()
    {
        ItemId = Guid.NewGuid().ToString();
        EventTime = DateTime.UtcNow;
    }
    
    public string ItemId { get; set; }
    public string EventType { get; set; } = "";
    
    public string EventData { get; set; } = "";
    
    public DateTime EventTime { get; set; }

    public bool Processed { get; set; } = false;
}