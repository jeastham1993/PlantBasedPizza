using PlantBasedPizza.Kitchen.Api.ViewModel;

namespace PlantBasedPizza.Kitchen.Api.Services;

public interface IQueueManager
{
    public Task StoreToQueue(KitchenRequest message);

    public Task CheckQueueStatus();
}