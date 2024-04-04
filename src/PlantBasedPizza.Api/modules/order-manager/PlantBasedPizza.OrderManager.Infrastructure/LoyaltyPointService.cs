using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class LoyaltyPointService : ILoyaltyPointService
{
    private readonly Loyalty.LoyaltyClient _loyaltyClient;
    private readonly ILogger<LoyaltyPointService> _logger;

    public LoyaltyPointService(ILogger<LoyaltyPointService> logger, Loyalty.LoyaltyClient loyaltyClient)
    {
        _logger = logger;
        _loyaltyClient = loyaltyClient;
    }

    public async Task AddLoyaltyPoints(string customerId, string orderIdentifier, decimal orderValue)
    {
        try
        {
            var createLoyaltyPointsResult = await this._loyaltyClient.AddLoyaltyPointsAsync(
                new AddLoyaltyPointsRequest()
                {
                    CustomerIdentifier = customerId,
                    OrderIdentifier = orderIdentifier,
                    OrderValue = (double)orderValue,

                });

            if (createLoyaltyPointsResult is null)
            {
                throw new Exception("Failure sending loyalty points");
            }
        }
        catch (Exception e)
        {
            this._logger.LogInformation(e, "Failure");
            throw;
        }
    }
}

public record CreateLoyaltyPointRequest(string CustomerIdentifier, string OrderIdentifier, decimal OrderValue);