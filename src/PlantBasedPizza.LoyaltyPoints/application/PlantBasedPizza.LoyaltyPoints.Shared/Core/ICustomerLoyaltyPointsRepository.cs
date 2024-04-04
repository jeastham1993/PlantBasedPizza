namespace PlantBasedPizza.LoyaltyPoints.Shared.Core;

public interface ICustomerLoyaltyPointsRepository
{
    Task<CustomerLoyaltyPoints?> GetCurrentPointsFor(string customerIdentifier);
    
    Task UpdatePoints(CustomerLoyaltyPoints points);
}