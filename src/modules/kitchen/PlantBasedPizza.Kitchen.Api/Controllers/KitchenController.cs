using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Api.Services;
using PlantBasedPizza.Kitchen.Api.ViewModel;

namespace PlantBasedPizza.Kitchen.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class KitchenController : ControllerBase
{
    private readonly ILogger<KitchenController> _logger;
    private readonly IQueueManager _queueManager;
    private readonly IMetrics _metrics;
    public KitchenController(ILogger<KitchenController> logger, IQueueManager queueManager, IMetrics metrics)
    {
        _logger = logger;
        _queueManager = queueManager;
        _metrics = metrics;
    }

    [HttpPost(Name = "StartKitchenPrep")]
    public async Task<IActionResult> StartKitchenPrep([FromBody] KitchenRequest request)
    {
        try
        {
            this._logger.LogInformation("Request received to kitchen API");
        
            if (string.IsNullOrEmpty(request.OrderNumber))
            {
                this._logger.LogInformation("Order number cannot be blank, returning error");
            
                return this.BadRequest(new ResponseWrapper()
                {
                    Message = "Order number cannot be blank"
                });
            }

            this._logger.LogInformation($"Sending request for order {request.OrderNumber} to queue");

            await this._queueManager.StoreToQueue(request);

            this._logger.LogInformation("Done");

            await this._metrics.IncrementMetric("OrderReceived");

            return this.Created(string.Empty, new ResponseWrapper()
            {
                Message = "Stored response"
            });
        }
        catch (Exception e)
        {
            await this._metrics.IncrementMetric("OrderReciptError");
            
            this._logger.LogError(e, "Failure processing request");

            return this.BadRequest(new ResponseWrapper()
            {
                Message = $"{e.Message} {e.StackTrace}"
            });
        }
    }
}