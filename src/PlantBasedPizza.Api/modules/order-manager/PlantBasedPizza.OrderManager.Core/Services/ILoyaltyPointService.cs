namespace PlantBasedPizza.OrderManager.Core.Services;

public interface ILoyaltyPointService
{
    Task<decimal> GetCustomerLoyaltyPoints(string customerId);
}