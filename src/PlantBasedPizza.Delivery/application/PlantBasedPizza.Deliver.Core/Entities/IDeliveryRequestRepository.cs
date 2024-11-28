using PlantBasedPizza.Events;

namespace PlantBasedPizza.Deliver.Core.Entities
{
    public interface IDeliveryRequestRepository
    {
        Task AddNewDeliveryRequest(DeliveryRequest request, List<IntegrationEvent> events = null);
        
        Task UpdateDeliveryRequest(DeliveryRequest request, List<IntegrationEvent> events = null);

        Task<DeliveryRequest?> GetDeliveryStatusForOrder(string orderIdentifier);

        Task<List<DeliveryRequest>> GetAwaitingDriver();

        Task<List<DeliveryRequest>> GetOrdersWithDriver(string driverName);
    }
}