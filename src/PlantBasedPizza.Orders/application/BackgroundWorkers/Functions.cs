using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.SQSEvents;
using BackgroundWorkers.Handlers;
using BackgroundWorkers.IntegrationEvents;
using Datadog.Trace;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using SpanContext = Datadog.Trace.SpanContext;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BackgroundWorkers;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions
{
    private readonly SqsEventSubscriber _eventSubscriber;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<Functions> _logger;
    private readonly DriverCollectedOrderEventHandler _driverCollectedOrderEventHandler;
    private readonly DriverDeliveredOrderEventHandler _driverDeliveredOrderEventHandler;
    private readonly OrderBakedEventHandler _orderBakedEventHandler;
    private readonly OrderPreparingEventHandler _orderPreparingEventHandler;
    private readonly OrderPrepCompleteEventHandler _orderPrepCompleteEventHandler;
    private readonly OrderQualityCheckedEventHandler _orderQualityCheckedEventHandler;
    private readonly PaymentSuccessfulEventHandler _paymentSuccessfulEventHandler;
    
    public Functions(SqsEventSubscriber eventSubscriber, IDistributedCache distributedCache, ILogger<Functions> logger, DriverCollectedOrderEventHandler driverCollectedOrderEventHandler, DriverDeliveredOrderEventHandler driverDeliveredOrderEventHandler, OrderPrepCompleteEventHandler orderPrepCompleteEventHandler, OrderPreparingEventHandler orderPreparingEventHandler, OrderBakedEventHandler orderBakedEventHandler, OrderQualityCheckedEventHandler orderQualityCheckedEventHandler, PaymentSuccessfulEventHandler paymentSuccessfulEventHandler)
    {
        _eventSubscriber = eventSubscriber;
        _distributedCache = distributedCache;
        _logger = logger;
        _driverCollectedOrderEventHandler = driverCollectedOrderEventHandler;
        _driverDeliveredOrderEventHandler = driverDeliveredOrderEventHandler;
        _orderPrepCompleteEventHandler = orderPrepCompleteEventHandler;
        _orderPreparingEventHandler = orderPreparingEventHandler;
        _orderBakedEventHandler = orderBakedEventHandler;
        _orderQualityCheckedEventHandler = orderQualityCheckedEventHandler;
        _paymentSuccessfulEventHandler = paymentSuccessfulEventHandler;
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
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleLoyaltyPointsUpdate",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });
                    
                    await _distributedCache.SetStringAsync(message.EventData.CustomerIdentifier.ToUpper(),
                        message.EventData.TotalLoyaltyPoints.ToString("n0"));
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
            batchItemFailures.AddRange(sqsEvent.Records.Select(record => new SQSBatchResponse.BatchItemFailure(){ItemIdentifier = record.MessageId}).ToList());
            this._logger.LogError(ex, "Failure handling SQS messages");
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
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleDriverCollectedOrder",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    await this._driverCollectedOrderEventHandler.Handle(message.EventData);
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
            batchItemFailures.AddRange(sqsEvent.Records.Select(record => new SQSBatchResponse.BatchItemFailure(){ItemIdentifier = record.MessageId}).ToList());
            this._logger.LogError(ex, "Failure handling SQS messages");
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
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleDriverDeliveredEvent",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    await this._driverDeliveredOrderEventHandler.Handle(message.EventData);
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
            batchItemFailures.AddRange(sqsEvent.Records.Select(record => new SQSBatchResponse.BatchItemFailure(){ItemIdentifier = record.MessageId}).ToList());
            this._logger.LogError(ex, "Failure handling SQS messages");
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
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleOrderBakedEvent",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                 
                    await this._orderBakedEventHandler.Handle(message.EventData);
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
            batchItemFailures.AddRange(sqsEvent.Records.Select(record => new SQSBatchResponse.BatchItemFailure(){ItemIdentifier = record.MessageId}).ToList());
            this._logger.LogError(ex, "Failure handling SQS messages");
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
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleOrderPreparingEvent",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });
       
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);

                    await this._orderPreparingEventHandler.Handle(message.EventData);
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
            batchItemFailures.AddRange(sqsEvent.Records.Select(record => new SQSBatchResponse.BatchItemFailure(){ItemIdentifier = record.MessageId}).ToList());
            this._logger.LogError(ex, "Failure handling SQS messages");
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
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleOrderPrepCompleteEvent",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);
                    
                    await this._orderPrepCompleteEventHandler.Handle(message.EventData);
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
            batchItemFailures.AddRange(sqsEvent.Records.Select(record => new SQSBatchResponse.BatchItemFailure(){ItemIdentifier = record.MessageId}).ToList());
            this._logger.LogError(ex, "Failure handling SQS messages");
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
                try
                {
                    using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("HandleQualityCheckedEvent",
                        new SpanCreationSettings()
                        {
                            Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                        });
                    
                    this._logger.LogInformation("Processing {messageId}", message.MessageId);

                    await this._orderQualityCheckedEventHandler.Handle(message.EventData);
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
            batchItemFailures.AddRange(sqsEvent.Records.Select(record => new SQSBatchResponse.BatchItemFailure(){ItemIdentifier = record.MessageId}).ToList());
            this._logger.LogError(ex, "Failure handling SQS messages");
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
                using var parent_trace = Datadog.Trace.Tracer.Instance.StartActive("PaymentSuccessfulEventHandler",
                    new SpanCreationSettings()
                    {
                        Parent = new SpanContext(message.TraceId, message.SpanId, SamplingPriority.AutoKeep)
                    });
                
                try
                {
                    await this._paymentSuccessfulEventHandler.Handle(message.EventData);
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
            batchItemFailures.AddRange(sqsEvent.Records.Select(record => new SQSBatchResponse.BatchItemFailure(){ItemIdentifier = record.MessageId}).ToList());
            this._logger.LogError(ex, "Failure handling SQS messages");
        }

        return new SQSBatchResponse(batchItemFailures);
    }
}