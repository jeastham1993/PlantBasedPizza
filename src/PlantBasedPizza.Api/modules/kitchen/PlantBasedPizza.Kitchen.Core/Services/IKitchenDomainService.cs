using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Core.Services;

public interface IKitchenDomainService
{
    Task StartPreparingAsync(KitchenRequest request, string correlationId = "");
    Task CompletePreparationAsync(KitchenRequest request, string correlationId = "");
    Task CompleteBakingAsync(KitchenRequest request, string correlationId = "");
    Task CompleteQualityCheckAsync(KitchenRequest request, string correlationId = "");
}