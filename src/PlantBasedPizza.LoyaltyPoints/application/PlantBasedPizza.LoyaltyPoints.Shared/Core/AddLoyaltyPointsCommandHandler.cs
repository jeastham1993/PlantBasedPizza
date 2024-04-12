using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;

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
        this._logger.LogInformation($"Handling AddLoyaltyPointsCommand for {command.OrderIdentifier}");
        
        var currentLoyaltyPoints = await this._customerLoyaltyPointsRepository.GetCurrentPointsFor(command.CustomerIdentifier);
        
        if (currentLoyaltyPoints is null)
        {
            currentLoyaltyPoints = CustomerLoyaltyPoints.Create(command.CustomerIdentifier);   
        }
        
        currentLoyaltyPoints.AddLoyaltyPoints(command.OrderValue, command.OrderIdentifier);

        await this._customerLoyaltyPointsRepository.UpdatePoints(currentLoyaltyPoints);

        return new LoyaltyPointsDto(currentLoyaltyPoints);
    }
}