using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantBasedPizza.Kitchen.Core.Entities
{
    public interface IKitchenRequestRepository
    {
        Task AddNew(KitchenRequest kitchenRequest);
        
        Task Update(KitchenRequest kitchenRequest);

        Task<KitchenRequest> Retrieve(string orderIdentifier);
        
        Task<IEnumerable<KitchenRequest>> GetNew();

        Task<IEnumerable<KitchenRequest>> GetPrep();

        Task<IEnumerable<KitchenRequest>> GetBaking();

        Task<IEnumerable<KitchenRequest>> GetAwaitingQualityCheck();
    }
}