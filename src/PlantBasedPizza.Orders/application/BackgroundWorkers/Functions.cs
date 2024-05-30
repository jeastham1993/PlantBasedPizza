using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using BackgroundWorkers.Handlers;
using BackgroundWorkers.IntegrationEvents;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using PlantBasedPizza.Events;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BackgroundWorkers;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions
{
    private readonly SqsEventSubscriber _eventSubscriber;
    private readonly IDistributedCache _distributedCache;
    private readonly TracerProvider _tracerProvider;
    private readonly ActivitySource _source;
    private readonly ILogger<Functions> _logger;
    private readonly DriverCollectedOrderEventHandler _driverCollectedOrderEventHandler;
    private readonly DriverDeliveredOrderEventHandler _driverDeliveredOrderEventHandler;
    private readonly OrderBakedEventHandler _orderBakedEventHandler;
    private readonly OrderPreparingEventHandler _orderPreparingEventHandler;
    private readonly OrderPrepCompleteEventHandler _orderPrepCompleteEventHandler;
    private readonly OrderQualityCheckedEventHandler _orderQualityCheckedEventHandler;
    private readonly PaymentSuccessfulEventHandler _paymentSuccessfulEventHandler;
    
    public Functions(SqsEventSubscriber eventSubscriber, IDistributedCache distributedCache, TracerProvider tracerProvider, ILogger<Functions> logger, DriverCollectedOrderEventHandler driverCollectedOrderEventHandler, DriverDeliveredOrderEventHandler driverDeliveredOrderEventHandler, OrderPrepCompleteEventHandler orderPrepCompleteEventHandler, OrderPreparingEventHandler orderPreparingEventHandler, OrderBakedEventHandler orderBakedEventHandler, OrderQualityCheckedEventHandler orderQualityCheckedEventHandler, PaymentSuccessfulEventHandler paymentSuccessfulEventHandler)
    {
        _eventSubscriber = eventSubscriber;
        _distributedCache = distributedCache;
        _tracerProvider = tracerProvider;
        _logger = logger;
        _driverCollectedOrderEventHandler = driverCollectedOrderEventHandler;
        _driverDeliveredOrderEventHandler = driverDeliveredOrderEventHandler;
        _orderPrepCompleteEventHandler = orderPrepCompleteEventHandler;
        _orderPreparingEventHandler = orderPreparingEventHandler;
        _orderBakedEventHandler = orderBakedEventHandler;
        _orderQualityCheckedEventHandler = orderQualityCheckedEventHandler;
        _paymentSuccessfulEventHandler = paymentSuccessfulEventHandler;
        _source = new ActivitySource(Environment.GetEnvironmentVariable("SERVICE_NAME"));;
    }

    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleLoyaltyPointsUpdate(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<CustomerLoyaltyPointsUpdatedEvent>(sqsEvent.Records);

            foreach (var message in messages)
            {
                using var processingActivity = _source.StartActivity("processing-loyalty-points-updated-event",
                    ActivityKind.Server, message.TraceParent);
                
                try
                {
                    context.AddToTrace();

                    using var queueActivity = _source.StartActivity("queue-time", ActivityKind.Internal,
                        message.TraceParent, startTime: message.EventPublishDate);
                    queueActivity.Stop();
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("customerIdentifier", message.EventData.CustomerIdentifier);
                    processingActivity?.AddTag("totalPoints", message.EventData.TotalLoyaltyPoints);

                    await _distributedCache.SetStringAsync(message.EventData.CustomerIdentifier.ToUpper(),
                        message.EventData.TotalLoyaltyPoints.ToString("n0"));
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
    

    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleDriverCollectedOrder(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<DriverCollectedOrderEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {
                using var processingActivity = _source.StartActivity("processing-driver-collected-order-event",
                                     ActivityKind.Server, message.TraceParent);
                try
                {
                    context.AddToTrace();

                    using var queueActivity = _source.StartActivity("queue-time", ActivityKind.Internal,
                        message.TraceParent, startTime: message.EventPublishDate);
                    queueActivity.Stop();
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await this._driverCollectedOrderEventHandler.Handle(message.EventData);
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
    
    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleDriverDeliveredOrder(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<DriverDeliveredOrderEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {
                using var processingActivity = _source.StartActivity("processing-driver-delivered-order-event",
                                     ActivityKind.Server, message.TraceParent);
                try
                {
                    context.AddToTrace();

                    using var queueActivity = _source.StartActivity("queue-time", ActivityKind.Internal,
                        message.TraceParent, startTime: message.EventPublishDate);
                    queueActivity.Stop();
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await this._driverDeliveredOrderEventHandler.Handle(message.EventData);
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
    
    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleOrderBakedEvent(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<OrderBakedEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {
                using var processingActivity = _source.StartActivity("processing-order-baked-event",
                                     ActivityKind.Server, message.TraceParent);
                try
                {
                    context.AddToTrace();

                    using var queueActivity = _source.StartActivity("queue-time", ActivityKind.Internal,
                        message.TraceParent, startTime: message.EventPublishDate);
                    queueActivity.Stop();
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await this._orderBakedEventHandler.Handle(message.EventData);
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
    
    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleOrderPreparingEvent(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<OrderPreparingEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {
                using var processingActivity = _source.StartActivity("processing-order-baked-event",
                    ActivityKind.Server, message.TraceParent);
                
                try
                {
                    context.AddToTrace();

                    using var queueActivity = _source.StartActivity("queue-time", ActivityKind.Internal,
                        message.TraceParent, startTime: message.EventPublishDate);
                    queueActivity.Stop();
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await this._orderPreparingEventHandler.Handle(message.EventData);
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
    
    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleOrderPrepCompleteEvent(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<OrderPrepCompleteEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {
                using var processingActivity = _source.StartActivity("processing-order-prep-complete-event",
                                     ActivityKind.Server, message.TraceParent);
                try
                {
                    context.AddToTrace();

                    using var queueActivity = _source.StartActivity("queue-time", ActivityKind.Internal,
                        message.TraceParent, startTime: message.EventPublishDate);
                    queueActivity.Stop();
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await this._orderPrepCompleteEventHandler.Handle(message.EventData);
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
    
    [LambdaFunction]
    public async Task<SQSBatchResponse> HandleOrderQualityCheckedEvent(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<OrderQualityCheckedEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {
                context.AddToTrace();

                using var queueActivity = _source.StartActivity("queue-time", ActivityKind.Internal,
                    message.TraceParent, startTime: message.EventPublishDate);
                queueActivity.Stop();
                
                using var processingActivity = _source.StartActivity("processing-order-baked-event",
                                     ActivityKind.Server, message.TraceParent);
                try
                {
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await this._orderQualityCheckedEventHandler.Handle(message.EventData);
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
    
    [LambdaFunction]
    public async Task<SQSBatchResponse> HandlePaymentSuccessfulEvent(SQSEvent sqsEvent, ILambdaContext context)
    {
        var batchItemFailures = new List<SQSBatchResponse.BatchItemFailure>();
        
        try
        {
            var messages = await _eventSubscriber.ParseMessages<PaymentSuccessfulEventV1>(sqsEvent.Records);

            foreach (var message in messages)
            {
                context.AddToTrace();

                using var queueActivity = _source.StartActivity("queue-time", ActivityKind.Internal,
                    message.TraceParent, startTime: message.EventPublishDate);
                queueActivity.Stop();
                
                using var processingActivity = _source.StartActivity("processing-payment-success-event",
                                     ActivityKind.Server, message.TraceParent);
                try
                {
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    processingActivity?.AddTag("queue.time", message.QueueTime);
                    processingActivity?.AddTag("orderIdentifier", message.EventData.OrderIdentifier);

                    await this._paymentSuccessfulEventHandler.Handle(message.EventData);
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