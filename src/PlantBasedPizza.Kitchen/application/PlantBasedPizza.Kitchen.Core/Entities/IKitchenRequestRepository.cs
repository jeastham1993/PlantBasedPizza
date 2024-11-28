using PlantBasedPizza.Events;

namespace PlantBasedPizza.Kitchen.Core.Entities
{
    public interface IKitchenRequestRepository
    {
        Task AddNew(KitchenRequest kitchenRequest, List<IntegrationEvent> events = null);
        
        Task Update(KitchenRequest kitchenRequest, List<IntegrationEvent> events = null);

        Task<KitchenRequest> Retrieve(string orderIdentifier);
        
        Task<IEnumerable<KitchenRequest>> GetNew();

        Task<IEnumerable<KitchenRequest>> GetPrep();

        Task<IEnumerable<KitchenRequest>> GetBaking();

        Task<IEnumerable<KitchenRequest>> GetAwaitingQualityCheck();
    }
}