using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;
using PlantBasedPizza.Kitchen.Infrastructure.IntegrationEvents;

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
    
    public static async Task<KitchenRequestDto> MarkPreparing([FromServices] IKitchenRequestRepository kitchenRequestRepository, [FromServices] KitchenEventPublisher eventPublisher, string orderIdentifier)
    {
        var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

        kitchenRequest.Preparing();

        await kitchenRequestRepository.Update(kitchenRequest);
        await eventPublisher.PublishOrderPreparingEventV1(kitchenRequest);

        return new KitchenRequestDto(kitchenRequest);
    }
    
    public static async Task<KitchenRequestDto> MarkPrepComplete([FromServices] IKitchenRequestRepository kitchenRequestRepository, [FromServices] KitchenEventPublisher eventPublisher, string orderIdentifier)
    {
        var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

        kitchenRequest.PrepComplete();

        await kitchenRequestRepository.Update(kitchenRequest);
        await eventPublisher.PublishOrderPrepCompleteEventV1(kitchenRequest);

        return new KitchenRequestDto(kitchenRequest);
    }
    
    public static async Task<KitchenRequestDto> MarkBakeComplete([FromServices] IKitchenRequestRepository kitchenRequestRepository, [FromServices] KitchenEventPublisher eventPublisher, string orderIdentifier)
    {
        var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

        kitchenRequest.BakeComplete();

        await kitchenRequestRepository.Update(kitchenRequest);
        await eventPublisher.PublishOrderBakedEventV1(kitchenRequest);

        return new KitchenRequestDto(kitchenRequest);
    }
    
    public static async Task<KitchenRequestDto> MarkQualityChecked([FromServices] IKitchenRequestRepository kitchenRequestRepository, [FromServices] KitchenEventPublisher eventPublisher, string orderIdentifier)
    {
        var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

        await kitchenRequest.QualityCheckComplete();

        await kitchenRequestRepository.Update(kitchenRequest);
        await eventPublisher.PublishOrderQualityCheckedEventV1(kitchenRequest);

        return new KitchenRequestDto(kitchenRequest);
    }
}