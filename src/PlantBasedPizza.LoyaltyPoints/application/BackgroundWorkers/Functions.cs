using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using BackgroundWorkers.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Datadog.Trace;
using PlantBasedPizza.Events;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BackgroundWorkers;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions
{
    private readonly SqsEventSubscriber _eventSubscriber;
    private readonly ILogger<Functions> _logger;
    private readonly AddLoyaltyPointsCommandHandler _addLoyaltyPointsCommandHandler;
    
    public Functions(SqsEventSubscriber eventSubscriber, ILogger<Functions> logger, AddLoyaltyPointsCommandHandler addLoyaltyPointsCommandHandler)
    {
        _eventSubscriber = eventSubscriber;
        _logger = logger;
        _addLoyaltyPointsCommandHandler = addLoyaltyPointsCommandHandler;
    }

    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleOrderCompleted(SQSEvent sqsEvent)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<OrderCompletedEvent>(sqsEvent.Records);

            foreach (var message in messages)
            {
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleOrderCompleted",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });

                    this._logger.LogInformation("Processing {messageId}", message.MessageId);

                    await _addLoyaltyPointsCommandHandler.Handle(new AddLoyaltyPointsCommand
                    {
                        CustomerIdentifier = message.EventData.CustomerIdentifier,
                        OrderValue = message.EventData.OrderValue,
                        OrderIdentifier = message.EventData.OrderIdentifier
                    });
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Failure handling SQS messages");
                    batchItemFailures.Add(new SQSBatchResponse.BatchItemFailure()
                    {
                        ItemIdentifier = message.MessageId
                    });
                }
            }

        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failure handling SQS messages");
        }

        return new SQSBatchResponse(batchItemFailures);
    }
}