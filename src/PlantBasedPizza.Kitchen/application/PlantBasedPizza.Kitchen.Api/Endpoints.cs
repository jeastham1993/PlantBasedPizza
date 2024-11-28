using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.PublicEvents;

namespace PlantBasedPizza.Kitchen.Api;

public static class Endpoints
{
    public static async Task<IEnumerable<KitchenRequestDto>> GetNew([FromServices] IKitchenRequestRepository kitchenRequestRepository)
    {
        try
        {
            var queryResults = await kitchenRequestRepository.GetNew();

            return queryResults.Select(p => new KitchenRequestDto(p)).ToList();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            return new List<KitchenRequestDto>();
        }
    }
    
    public static async Task<IEnumerable<KitchenRequestDto>> GetPrep([FromServices] IKitchenRequestRepository kitchenRequestRepository)
    {
        try
        {
            var queryResults = await kitchenRequestRepository.GetPrep();

            return queryResults.Select(p => new KitchenRequestDto(p)).ToList();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            return new List<KitchenRequestDto>();
        }
    }
    
    public static async Task<IEnumerable<KitchenRequestDto>> GetBaking([FromServices] IKitchenRequestRepository kitchenRequestRepository)
    {
        try
        {
            var queryResults = await kitchenRequestRepository.GetBaking();

            return queryResults.Select(p => new KitchenRequestDto(p)).ToList();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            return new List<KitchenRequestDto>();
        }
    }
    
    public static async Task<IEnumerable<KitchenRequestDto>> GetAwaitingQualityCheck([FromServices] IKitchenRequestRepository kitchenRequestRepository)
    {
        try
        {
            var queryResults = await kitchenRequestRepository.GetAwaitingQualityCheck();

            return queryResults.Select(p => new KitchenRequestDto(p)).ToList();
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            return new List<KitchenRequestDto>();
        }
    }
    
    public static async Task<KitchenRequestDto> MarkPreparing([FromServices] IKitchenRequestRepository kitchenRequestRepository, string orderIdentifier)
    {
        Activity.Current?.AddTag("orderIdentifier", orderIdentifier);
        
        var kitchenRequest = await kitchenRequestRepository.Retrieve(orderIdentifier);

        kitchenRequest.Preparing();

        await kitchenRequestRepository.Update(kitchenRequest, [new OrderPreparingEventV1()
        {
            OrderIdentifier = orderIdentifier
        }]);

        return new KitchenRequestDto(kitchenRequest);
    }
    
    public static async Task<KitchenRequestDto> MarkPrepComplete([FromServices] IKitchenRequestRepository kitchenRequestRepository, string orderIdentifier)
    {
        Activity.Current?.AddTag("orderIdentifier", orderIdentifier);
        
        var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

        kitchenRequest.PrepComplete();

        await kitchenRequestRepository.Update(kitchenRequest, [new OrderPrepCompleteEventV1()
        {
            OrderIdentifier = orderIdentifier
        }]);

        return new KitchenRequestDto(kitchenRequest);
    }
    
    public static async Task<KitchenRequestDto> MarkBakeComplete([FromServices] IKitchenRequestRepository kitchenRequestRepository, string orderIdentifier)
    {
        Activity.Current?.AddTag("orderIdentifier", orderIdentifier);
        
        var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

        kitchenRequest.BakeComplete();

        await kitchenRequestRepository.Update(kitchenRequest, [new OrderBakedEventV1()
        {
            OrderIdentifier = orderIdentifier
        }]);

        return new KitchenRequestDto(kitchenRequest);
    }
    
    public static async Task<KitchenRequestDto> MarkQualityChecked([FromServices] IKitchenRequestRepository kitchenRequestRepository, string orderIdentifier)
    {
        Activity.Current?.AddTag("orderIdentifier", orderIdentifier);
        
        var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

        await kitchenRequest.QualityCheckComplete();

        await kitchenRequestRepository.Update(kitchenRequest, [new OrderQualityCheckedEventV1()
        {
            OrderIdentifier = orderIdentifier
        }]);

        return new KitchenRequestDto(kitchenRequest);
    }
}