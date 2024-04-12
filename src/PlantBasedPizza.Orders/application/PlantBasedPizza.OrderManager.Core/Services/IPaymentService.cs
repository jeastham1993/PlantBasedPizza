using PlantBasedPizza.OrderManager.Core.Entities;

namespace PlantBasedPizza.OrderManager.Core.Services;

public interface IPaymentService
{
    Task<TakePaymentResult> TakePaymentFor(Order order);
}