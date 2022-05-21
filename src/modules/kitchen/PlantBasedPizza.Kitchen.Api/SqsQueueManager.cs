using System.Net;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using CorrelationId.Abstractions;
using PlantBasedPizza.Kitchen.Api.Controllers;
using PlantBasedPizza.Kitchen.Api.Services;
using PlantBasedPizza.Kitchen.Api.ViewModel;

namespace PlantBasedPizza.Kitchen.Api;

public class SqsQueueManager : IQueueManager
{
    private readonly ILogger<SqsQueueManager> _logger;
    private readonly ICorrelationContextAccessor _correlationContext;
    private readonly Settings _settings;
    private readonly AmazonSQSClient _sqsClient;
    public SqsQueueManager(ILogger<SqsQueueManager> logger, Settings settings, AmazonSQSClient sqsClient, ICorrelationContextAccessor correlationContext)
    {
        _logger = logger;
        _settings = settings;
        _sqsClient = sqsClient;
        _correlationContext = correlationContext;
    }
    public async Task StoreToQueue(KitchenRequest message)
    {
        var queueUrl = this._settings.QueueUrls[message.LocationCode];
        
        this._logger.LogInformation($"Publishing to {queueUrl}");
        
        var result = await this._sqsClient.SendMessageAsync(new SendMessageRequest()
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(new QueuedMessage()
            {
                Payload = message,
                CorrelationId = this._correlationContext.CorrelationContext.CorrelationId
            }),
        });
    }

    public async Task CheckQueueStatus()
    {
        foreach (var queue in this._settings.QueueUrls)
        {
            var attributes = await this._sqsClient.GetQueueAttributesAsync(queue.Value, new List<string>());
        
            if (attributes.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("SQS queue offline");
            }   
        }
    }
}