using Grpc.Core;
using PlantBasedPizza.Api.Internal;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Internal.Services;

public class LoyaltyService : Loyalty.LoyaltyBase
{
    private readonly AddLoyaltyPointsCommandHandler _handler;

    public LoyaltyService(AddLoyaltyPointsCommandHandler handler)
    {
        _handler = handler;
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