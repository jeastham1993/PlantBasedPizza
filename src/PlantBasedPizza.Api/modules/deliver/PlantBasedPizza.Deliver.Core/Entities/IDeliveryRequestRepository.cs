using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantBasedPizza.Deliver.Core.Entities
{
    public interface IDeliveryRequestRepository
    {
        Task AddNewDeliveryRequest(DeliveryRequest request);
        
        Task UpdateDeliveryRequest(DeliveryRequest request);

        Task<DeliveryRequest?> GetDeliveryStatusForOrder(string orderIdentifier);

        Task<List<DeliveryRequest>> GetAwaitingDriver();

        Task<List<DeliveryRequest>> GetOrdersWithDriver(string driverName);
    }
}