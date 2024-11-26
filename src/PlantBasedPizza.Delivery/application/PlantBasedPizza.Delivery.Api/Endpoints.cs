using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Deliver.Core.AssignDriver;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.GetAwaitingCollection;
using PlantBasedPizza.Deliver.Core.GetDeliveryStatus;
using PlantBasedPizza.Deliver.Core.MarkOrderDelivered;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Delivery.Api;

public static class Endpoints
{
    public static async Task<DeliveryRequestDto> GetOrderStatus(ClaimsPrincipal user, [FromServices] GetDeliveryQueryHandler handler,
        string orderIdentifier)
    {
        user.Claims.ExtractAccountId();
        
        var response = await handler.Handle(new GetDeliveryQuery(orderIdentifier));

        return response;
    }

    public static async Task<IEnumerable<DeliveryRequestDto>> GetAwaitingCollection(
        ClaimsPrincipal user, [FromServices] GetAwaitingCollectionQueryHandler handler)
    {
        user.Claims.ExtractAccountId();
        
        var response = await handler.Handle(new GetAwaitingCollectionQuery());

        return response;
    }

    public static async Task<IEnumerable<DeliveryRequestDto>> GetOrdersForDriver(
        [FromServices] IDeliveryRequestRepository repository, string driverName)
    {
        var request =  await repository.GetOrdersWithDriver(driverName);
        
        return request.Select(r => new DeliveryRequestDto(r));
    }

    public static async Task<IResult> CollectOrder(ClaimsPrincipal user, [FromServices] AssignDriverRequestHandler handler, [FromBody] AssignDriverRequest request, CancellationToken token)
    {
        user.Claims.ExtractAccountId();
        request.AddToTelemetry();
        
        var result = await handler.Handle(request);
        
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
    
    public static async Task<IResult> MarkOrderDelivered(ClaimsPrincipal user, [FromServices] MarkOrderDeliveredRequestHandler handler, [FromBody] MarkOrderDeliveredRequest request)
    {
        user.Claims.ExtractAccountId();
        request.AddToTelemetry();
        
        var result = await handler.Handle(request);
        
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}