using System.Text.Json;
using Amazon.Runtime.SharedInterfaces;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Deliver.Infrastructure;

public class OrderReadyForDeliveryQueueProcessor : BackgroundService
{
    private readonly IObservabilityService _observabilityService;
    private readonly AmazonSQSClient _sqsClient;
    private readonly OrderReadyForDeliveryEventHandler _handler;

    public OrderReadyForDeliveryQueueProcessor(IObservabilityService observabilityService, AmazonSQSClient sqsClient, OrderReadyForDeliveryEventHandler handler)
    {
        _observabilityService = observabilityService;
        _sqsClient = sqsClient;
        _handler = handler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await this._sqsClient.ReceiveMessageAsync(InfrastructureConstants.OrderReadyForDeliveryQueue);

            var processedMessages = new List<DeleteMessageBatchRequestEntry>();
            
            foreach (var message in messages.Messages)
            {
                try
                {
                    _observabilityService.Info(message.Body);
                    
                    var evt = JsonSerializer.Deserialize<EventBridgeEvent<OrderReadyForDeliveryEvent>>(message.Body);

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
                    new DeleteMessageBatchRequest(InfrastructureConstants.OrderReadyForDeliveryQueue, processedMessages));   
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}