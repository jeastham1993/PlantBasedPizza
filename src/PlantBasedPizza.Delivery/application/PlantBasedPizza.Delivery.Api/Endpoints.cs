using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Deliver.Core.AssignDriver;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.GetAwaitingCollection;
using PlantBasedPizza.Deliver.Core.GetDeliveryStatus;
using PlantBasedPizza.Deliver.Core.MarkOrderDelivered;
using PlantBasedPizza.Deliver.Infrastructure;

namespace PlantBasedPizza.Delivery.Api;

public static class Endpoints
{
    public static async Task<DeliveryRequestDto> GetOrderStatus([FromServices] GetDeliveryQueryHandler handler,
        string orderIdentifier)
    {
        var response = await handler.Handle(new GetDeliveryQuery(orderIdentifier));

        return response;
    }

    public static async Task<IEnumerable<DeliveryRequestDto>> GetAwaitingCollection(
        [FromServices] GetAwaitingCollectionQueryHandler handler)
    {
        var response = await handler.Handle(new GetAwaitingCollectionQuery());

        return response;
    }

    public static async Task<IEnumerable<DeliveryRequestDto>> GetOrdersForDriver(
        [FromServices] IDeliveryRequestRepository repository, string driverName)
    {
        var request =  await repository.GetOrdersWithDriver(driverName);
        
        return request.Select(r => new DeliveryRequestDto(r));
    }

    public static async Task<IResult> AssignToDriver([FromServices] AssignDriverRequestHandler handler, [FromBody] AssignDriverRequest request, CancellationToken token)
    {
        request.AddToTelemetry();
        
        var result = await handler.Handle(request);
        
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
    
    public static async Task<IResult> MarkOrderDelivered([FromServices] MarkOrderDeliveredRequestHandler handler, [FromBody] MarkOrderDeliveredRequest request)
    {
        request.AddToTelemetry();
        
        var result = await handler.Handle(request);
        
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}