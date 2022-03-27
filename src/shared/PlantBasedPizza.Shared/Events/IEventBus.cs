namespace PlantBasedPizza.Shared.Events
{
    public interface IEventBus
    {
        Task Publish(BaseEvent evt);
    }
}
