namespace PlantBasedPizza.Events
{
    public interface IHandles<in T> where T : IDomainEvent
    {
        Task Handle(T evt);
    }
}