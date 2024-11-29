using Grpc.Core;
using PlantBasedPizza.Api.Internal;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Internal.Services;

public class LoyaltyService : Loyalty.LoyaltyBase
{
    private readonly AddLoyaltyPointsCommandHandler _handler;
    private readonly ICustomerLoyaltyPointsRepository _repository;
    private readonly ILogger<LoyaltyService> _logger;

    public LoyaltyService(AddLoyaltyPointsCommandHandler handler, ICustomerLoyaltyPointsRepository repository, ILogger<LoyaltyService> logger)
    {
        _handler = handler;
        _repository = repository;
        _logger = logger;
    }

    public override async Task<GetCustomerLoyaltyPointsReply> GetCustomerLoyaltyPoints(GetCustomerLoyaltyPointsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Handling GetCustomerLoyaltyPointsRequest for {CustomerIdentifier}", request.CustomerIdentifier);

        var loyaltyPoints = 0M;

        try
        {
            var loyaltyAccount = await _repository.GetCurrentPointsFor(request.CustomerIdentifier);
            loyaltyPoints = loyaltyAccount?.TotalPoints ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure retrieving loyalty account");
        }

        return new GetCustomerLoyaltyPointsReply()
        {
            CustomerIdentifier = request.CustomerIdentifier,
            TotalPoints = Convert.ToDouble(loyaltyPoints)
        };
    }

    public override async Task<AddLoyaltyPointsReply> AddLoyaltyPoints(AddLoyaltyPointsRequest request, ServerCallContext context)
    {
        var command = new AddLoyaltyPointsCommand()
        {
            OrderIdentifier = request.OrderIdentifier,
            CustomerIdentifier = request.CustomerIdentifier,
            OrderValue = (decimal)request.OrderValue
        };

        var result = await _handler.Handle(command);
        
        return new AddLoyaltyPointsReply()
        {
            CustomerIdentifier = request.CustomerIdentifier,
            TotalPoints = (double)result.TotalPoints
        };
    }
}