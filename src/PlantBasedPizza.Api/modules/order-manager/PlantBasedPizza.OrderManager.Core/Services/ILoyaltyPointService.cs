namespace PlantBasedPizza.OrderManager.Core.Services;

public interface ILoyaltyPointService
{
    Task AddLoyaltyPoints(string customerId, decimal orderValue);
}