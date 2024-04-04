namespace PlantBasedPizza.Api
{
    public interface Handles<in T> where T : IDomainEvent
    {
        Task Handle(T evt);
    }
}