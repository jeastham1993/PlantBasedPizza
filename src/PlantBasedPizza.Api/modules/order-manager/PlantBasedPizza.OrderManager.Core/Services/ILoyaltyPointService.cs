namespace PlantBasedPizza.OrderManager.Core.Services;

public interface ILoyaltyPointService
{
    Task AddLoyaltyPoints(string customerId, string orderIdentifier, decimal orderValue);

    Task<decimal> GetCustomerLoyaltyPoints(string customerId);
}