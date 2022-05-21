using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Handlers.ViewModels;

namespace PlantBasedPizza.Kitchen.Handlers;

public class Functions
{
    private ILogger<Functions> _logger;
    private IKitchenRequestRepository _kitchenRequestRepository;
    public Functions(ILogger<Functions> logger, IKitchenRequestRepository kitchenRequestRepository)
    {
        _logger = logger;
        _kitchenRequestRepository = kitchenRequestRepository;
    }
    
    [LambdaFunction]
    public async Task<string> QueueHandler(SQSEvent evt)
    {
        if (evt == null)
            throw new NullReferenceException();
        
        this._logger.LogInformation("[KITCHEN] Logging order submitted event");

        foreach (var record in evt.Records)
        {
            var request = JsonConvert.DeserializeObject<QueuedMessage>(record.Body);

            await this._kitchenRequestRepository.AddNew(new KitchenRequest(request.Payload.OrderNumber,
                new List<RecipeAdapter>()));
        }

        return string.Empty;
    }
}