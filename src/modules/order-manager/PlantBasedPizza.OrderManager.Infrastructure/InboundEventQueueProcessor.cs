using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class InboundEventQueueProcessor<TEvent, THandler>
    where THandler : Handles<TEvent>
    where TEvent : IDomainEvent 
{
    private readonly IObservabilityService _observabilityService;
    private readonly AmazonSQSClient _sqsClient;
    private readonly THandler _handler;

    public InboundEventQueueProcessor(IObservabilityService observabilityService, AmazonSQSClient sqsClient, THandler handler)
    {
        _observabilityService = observabilityService;
        _sqsClient = sqsClient;
        _handler = handler;
    }
    
    public async Task ProcessAsync(string queueUrl, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await this._sqsClient.ReceiveMessageAsync(queueUrl);

            var processedMessages = new List<DeleteMessageBatchRequestEntry>();
            
            foreach (var message in messages.Messages)
            {
                try
                {
                    var evt = JsonSerializer.Deserialize<EventBridgeEvent<TEvent>>(message.Body);

                    await this._handler.Handle(evt.Detail);

                    processedMessages.Add(new DeleteMessageBatchRequestEntry(message.MessageId, message.ReceiptHandle));
                }
                catch (AmazonSQSException e)
                {
                    this._observabilityService.Error(e, $"SQS Error. Queue URL is {queueUrl}");
                }
                catch (Exception e)
                {
                    this._observabilityService.Error(e, "Failure processing message");
                }
            }

            if (processedMessages.Any())
            {
                await this._sqsClient.DeleteMessageBatchAsync(
                    new DeleteMessageBatchRequest(queueUrl, processedMessages));   
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}