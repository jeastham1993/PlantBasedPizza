using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Logging;
using Datadog.Trace;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Core.IntegrationEvents;
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
    private readonly OrderReadyForDeliveryEventHandler _orderReadyForDeliveryEventHandler;
    
    public Functions(SqsEventSubscriber eventSubscriber, ILogger<Functions> logger, OrderReadyForDeliveryEventHandler orderReadyForDeliveryEventHandler)
    {
        _eventSubscriber = eventSubscriber;
        _logger = logger;
        _orderReadyForDeliveryEventHandler = orderReadyForDeliveryEventHandler;
    }

    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleOrderReadyForDelivery(SQSEvent sqsEvent)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<OrderReadyForDeliveryEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {                
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleOrderReadyForDelivery",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });

                    this._logger.LogInformation("Processing {messageId}", message.MessageId);

                    await this._orderReadyForDeliveryEventHandler.Handle(message.EventData);
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