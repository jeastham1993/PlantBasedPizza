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
        var result = await this._sqsClient.SendMessageAsync(new SendMessageRequest()
        {
            QueueUrl = this._settings.QueueUrl,
            MessageBody = JsonSerializer.Serialize(new QueuedMessage()
            {
                Payload = message,
                CorrelationId = this._correlationContext.CorrelationContext.CorrelationId
            }),
            MessageGroupId = "inbound-orders",
            MessageDeduplicationId = message.OrderNumber
        });
    }

    public async Task CheckQueueStatus()
    {
        var attributes = await this._sqsClient.GetQueueAttributesAsync(this._settings.QueueUrl, new List<string>());

        if (attributes.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new Exception("SQS queue offline");
        }
    }
}