using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Errors;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Kitchen.Handlers.ViewModels;

namespace PlantBasedPizza.Kitchen.Handlers;

public class Functions
{
    private ILogger<Functions> _logger;
    private IKitchenRequestRepository _kitchenRequestRepository;
    private IWorkflowManager _workflowManager;
    
    public Functions(ILogger<Functions> logger, IKitchenRequestRepository kitchenRequestRepository, IWorkflowManager workflowManager)
    {
        _logger = logger;
        _kitchenRequestRepository = kitchenRequestRepository;
        _workflowManager = workflowManager;
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
            
            this._logger.LogInformation($"Processing request for {request.Payload.OrderNumber}");

            try
            {
                var kitchenRequest = new KitchenRequest(request.Payload.OrderNumber, new List<RecipeAdapter>());

                await this._kitchenRequestRepository.AddNew(kitchenRequest);

                this._logger.LogInformation("Starting workflow");
                
                await this._workflowManager.StartWorkflowExecution(kitchenRequest);
            }
            catch (OrderExistsException)
            {
                this._logger.LogWarning("Order exists, skipping save");
            }
        }

        return string.Empty;
    }
}