using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
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
    private readonly TracerProvider _tracerProvider;
    private readonly ActivitySource _source;
    private readonly ILogger<Functions> _logger;
    private readonly OrderReadyForDeliveryEventHandler _orderReadyForDeliveryEventHandler;
    
    public Functions(SqsEventSubscriber eventSubscriber, TracerProvider tracerProvider, ILogger<Functions> logger, OrderReadyForDeliveryEventHandler orderReadyForDeliveryEventHandler)
    {
        _eventSubscriber = eventSubscriber;
        _tracerProvider = tracerProvider;
        _logger = logger;
        _orderReadyForDeliveryEventHandler = orderReadyForDeliveryEventHandler;
        _source = new ActivitySource(Environment.GetEnvironmentVariable("SERVICE_NAME"));;
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
                using var processingActivity = _source.StartActivity("processing-order-ready-for-delivery-event",
                    ActivityKind.Server, message.TraceParent);
                
                try
                {
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await this._orderReadyForDeliveryEventHandler.Handle(message.EventData);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Failure handling SQS messages");
                    processingActivity.RecordException(ex);
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
        finally
        {
            this._tracerProvider.ForceFlush();   
        }

        return new SQSBatchResponse(batchItemFailures);
    }
}