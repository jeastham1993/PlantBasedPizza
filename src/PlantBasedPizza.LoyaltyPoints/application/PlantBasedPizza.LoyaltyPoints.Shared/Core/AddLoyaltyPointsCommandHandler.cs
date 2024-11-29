using Microsoft.Extensions.Logging;

namespace PlantBasedPizza.LoyaltyPoints.Shared.Core;

public class AddLoyaltyPointsCommandHandler
{
    private readonly ICustomerLoyaltyPointsRepository _customerLoyaltyPointsRepository;
    private readonly ILogger<AddLoyaltyPointsCommandHandler> _logger;

    public AddLoyaltyPointsCommandHandler(ICustomerLoyaltyPointsRepository customerLoyaltyPointsRepository, ILogger<AddLoyaltyPointsCommandHandler> logger)
    {
        _customerLoyaltyPointsRepository = customerLoyaltyPointsRepository;
        _logger = logger;
    }

    public async Task<LoyaltyPointsDto> Handle(AddLoyaltyPointsCommand command)
    {
        _logger.LogInformation("Handling AddLoyaltyPointsCommand for {OrderIdentifier}", command.OrderIdentifier);
        
        var currentLoyaltyPoints = await _customerLoyaltyPointsRepository.GetCurrentPointsFor(command.CustomerIdentifier);
        
        if (currentLoyaltyPoints is null)
        {
            currentLoyaltyPoints = CustomerLoyaltyPoints.Create(command.CustomerIdentifier);   
        }
        
        currentLoyaltyPoints.AddLoyaltyPoints(command.OrderValue, command.OrderIdentifier);

        await _customerLoyaltyPointsRepository.UpdatePoints(currentLoyaltyPoints);

        return new LoyaltyPointsDto(currentLoyaltyPoints);
    }
}