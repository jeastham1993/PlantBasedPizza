using Grpc.Core;
using PlantBasedPizza.Api.Internal;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

namespace PlantBasedPizza.LoyaltyPoints.Internal.Services;

public class LoyaltyService : Loyalty.LoyaltyBase
{
    private readonly AddLoyaltyPointsCommandHandler _handler;
    private readonly ICustomerLoyaltyPointsRepository _repository;

    public LoyaltyService(AddLoyaltyPointsCommandHandler handler, ICustomerLoyaltyPointsRepository repository)
    {
        _handler = handler;
        _repository = repository;
    }

    public override async Task<GetCustomerLoyaltyPointsReply> GetCustomerLoyaltyPoints(GetCustomerLoyaltyPointsRequest request, ServerCallContext context)
    {
        var loyaltyPoints = await _repository.GetCurrentPointsFor(request.CustomerIdentifier);

        return new GetCustomerLoyaltyPointsReply()
        {
            CustomerIdentifier = request.CustomerIdentifier,
            TotalPoints = Convert.ToDouble(loyaltyPoints?.TotalPoints ?? 0)
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