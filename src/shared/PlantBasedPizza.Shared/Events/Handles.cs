using System.Threading.Tasks;

namespace PlantBasedPizza.Shared.Events
{
    public interface Handles<T> where T : IDomainEvent
    {
        Task Handle(T evt);
    }
}