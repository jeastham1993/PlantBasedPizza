namespace PlantBasedPizza.LoyaltyPoints.Shared.Core;

public class SpendLoyaltyPointsCommandHandler
{
    private readonly ICustomerLoyaltyPointsRepository _customerLoyaltyPointsRepository;

    public SpendLoyaltyPointsCommandHandler(ICustomerLoyaltyPointsRepository customerLoyaltyPointsRepository)
    {
        _customerLoyaltyPointsRepository = customerLoyaltyPointsRepository;
    }

    public async Task<LoyaltyPointsDto> Handle(SpendLoyaltyPointsCommand command)
    {
        var currentLoyaltyPoints = await _customerLoyaltyPointsRepository.GetCurrentPointsFor(command.CustomerIdentifier);
    
        if (currentLoyaltyPoints is null)
        {
            currentLoyaltyPoints = CustomerLoyaltyPoints.Create(command.CustomerIdentifier);   
        }
    
        currentLoyaltyPoints.SpendPoints(command.PointsToSpend, command.OrderIdentifier);

        await _customerLoyaltyPointsRepository.UpdatePoints(currentLoyaltyPoints);

        return new LoyaltyPointsDto(currentLoyaltyPoints);
    }
}