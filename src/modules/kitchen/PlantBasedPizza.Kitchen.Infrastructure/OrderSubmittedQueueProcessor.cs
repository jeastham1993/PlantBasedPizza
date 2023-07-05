using System.Text.Json.Serialization;
using Amazon.EventBridge.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Handlers;
using PlantBasedPizza.Shared.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class OrderSubmittedQueueProcessor : BackgroundService
{
    private readonly IObservabilityService _observabilityService;
    private readonly AmazonSQSClient _sqsClient;
    private readonly OrderSubmittedEventHandler _handler;

    public OrderSubmittedQueueProcessor(IObservabilityService observabilityService, AmazonSQSClient sqsClient, OrderSubmittedEventHandler handler)
    {
        _observabilityService = observabilityService;
        _sqsClient = sqsClient;
        _handler = handler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await this._sqsClient.ReceiveMessageAsync(InfrastructureConstants.OrderSubmittedQueueUrl);

            this._observabilityService.Info($"Received {messages.Messages.Count} message(s)");

            var processedMessages = new List<DeleteMessageBatchRequestEntry>();
            
            foreach (var message in messages.Messages)
            {
                try
                {
                    var evt = JsonSerializer.Deserialize<EventBridgeEvent<OrderSubmittedEvent>>(message.Body);

                    await this._handler.Handle(evt.Detail);
                    
                    processedMessages.Add(new DeleteMessageBatchRequestEntry(message.MessageId, message.ReceiptHandle));
                }
                catch (Exception e)
                {
                    this._observabilityService.Error(e, "Failure processing message");
                }
            }

            if (processedMessages.Any())
            {
                await this._sqsClient.DeleteMessageBatchAsync(
                    new DeleteMessageBatchRequest(InfrastructureConstants.OrderSubmittedQueueUrl, processedMessages));   
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

public record EventBridgeEvent<T>
{
    [JsonPropertyName("detail")]
    public T Detail { get; set; }
}