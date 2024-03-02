using System.Threading.Tasks;

namespace PlantBasedPizza.Shared.Events
{
    public interface Handles<in T> where T : IDomainEvent
    {
        Task Handle(T evt);
    }
}