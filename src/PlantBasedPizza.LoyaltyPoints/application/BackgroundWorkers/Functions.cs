using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using BackgroundWorkers.IntegrationEvents;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
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
    private readonly TracerProvider _tracerProvider;
    private readonly ActivitySource _source;
    private readonly ILogger<Functions> _logger;
    private readonly AddLoyaltyPointsCommandHandler _addLoyaltyPointsCommandHandler;
    
    public Functions(SqsEventSubscriber eventSubscriber, TracerProvider tracerProvider, ILogger<Functions> logger, AddLoyaltyPointsCommandHandler addLoyaltyPointsCommandHandler)
    {
        _eventSubscriber = eventSubscriber;
        _tracerProvider = tracerProvider;
        _logger = logger;
        _addLoyaltyPointsCommandHandler = addLoyaltyPointsCommandHandler;
        _source = new ActivitySource(Environment.GetEnvironmentVariable("SERVICE_NAME"));;
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
                using var processingActivity = _source.StartActivity("processing-order-ready-for-delivery-event",
                    ActivityKind.Server, message.TraceParent);
                
                try
                {
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

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