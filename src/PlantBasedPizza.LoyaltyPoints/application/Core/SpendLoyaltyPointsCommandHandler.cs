namespace PlantBasedPizza.LoyaltyPoints.Core;

public class SpendLoyaltyPointsCommandHandler
{
    private readonly ICustomerLoyaltyPointsRepository _customerLoyaltyPointsRepository;

    public SpendLoyaltyPointsCommandHandler(ICustomerLoyaltyPointsRepository customerLoyaltyPointsRepository)
    {
        _customerLoyaltyPointsRepository = customerLoyaltyPointsRepository;
    }

    public async Task<LoyaltyPointsDTO> Handle(SpendLoyaltyPointsCommand command)
    {
        var currentLoyaltyPoints = await this._customerLoyaltyPointsRepository.GetCurrentPointsFor(command.CustomerIdentifier);
    
        if (currentLoyaltyPoints is null)
        {
            currentLoyaltyPoints = CustomerLoyaltyPoints.Create(command.CustomerIdentifier);   
        }
    
        currentLoyaltyPoints.SpendPoints(command.PointsToSpend, command.OrderIdentifier);

        await this._customerLoyaltyPointsRepository.UpdatePoints(currentLoyaltyPoints);

        return new LoyaltyPointsDTO(currentLoyaltyPoints);
    }
}