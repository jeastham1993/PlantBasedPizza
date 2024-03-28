namespace PlantBasedPizza.LoyaltyPoints.Core;

public class AddLoyaltyPointsCommandHandler
{
    private readonly ICustomerLoyaltyPointsRepository _customerLoyaltyPointsRepository;

    public AddLoyaltyPointsCommandHandler(ICustomerLoyaltyPointsRepository customerLoyaltyPointsRepository)
    {
        _customerLoyaltyPointsRepository = customerLoyaltyPointsRepository;
    }

    public async Task<LoyaltyPointsDTO> Handle(AddLoyaltyPointsCommand command)
    {
        var currentLoyaltyPoints = await this._customerLoyaltyPointsRepository.GetCurrentPointsFor(command.CustomerIdentifier);
        
        if (currentLoyaltyPoints is null)
        {
            currentLoyaltyPoints = CustomerLoyaltyPoints.Create(command.CustomerIdentifier);   
        }
        
        currentLoyaltyPoints.AddLoyaltyPoints(command.OrderValue, command.OrderIdentifier);

        await this._customerLoyaltyPointsRepository.UpdatePoints(currentLoyaltyPoints);

        return new LoyaltyPointsDTO(currentLoyaltyPoints);
    }
}