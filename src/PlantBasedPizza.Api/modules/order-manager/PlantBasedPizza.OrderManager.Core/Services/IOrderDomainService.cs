using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IOrderDomainService
{
    Task SubmitOrderAsync(Order order, string correlationId = "");
    Task CompleteOrderAsync(Order order, string correlationId = "");
    Task MarkOrderAwaitingCollectionAsync(Order order, string correlationId = "");
}