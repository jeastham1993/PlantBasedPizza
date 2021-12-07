using System;

namespace PlantBasedPizza.Shared.Events
{
    public interface IDomainEvent
    {
        string EventName { get; }
        
        string EventId { get; }
        
        DateTime EventDate { get; }
    }
}