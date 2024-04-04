namespace PlantBasedPizza.Api.Events
{
    public interface Handles<in T> where T : IDomainEvent
    {
        Task Handle(T evt);
    }
}