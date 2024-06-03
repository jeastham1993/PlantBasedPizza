using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using BackgroundWorkers.Handlers;
using BackgroundWorkers.IntegrationEvents;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Datadog.Trace;
using PlantBasedPizza.Events;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BackgroundWorkers;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions
{
    private readonly SqsEventSubscriber _eventSubscriber;
    private readonly ILogger<Functions> _logger;
    private readonly OrderConfirmedEventHandler _orderConfirmedEventHandler;
    
    public Functions(SqsEventSubscriber eventSubscriber, ILogger<Functions> logger, OrderConfirmedEventHandler orderConfirmedEventHandler)
    {
        _eventSubscriber = eventSubscriber;
        _logger = logger;
        _orderConfirmedEventHandler = orderConfirmedEventHandler;
    }

    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleOrderSubmitted(SQSEvent sqsEvent)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<OrderConfirmedEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {
                using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleOrderSubmittedEvent",
                    new SpanCreationSettings()
                    {
                        Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                    });
                
                try
                {
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);

                    await this._orderConfirmedEventHandler.Handle(message.EventData);
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