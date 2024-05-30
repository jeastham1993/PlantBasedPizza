using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Datadog.Trace;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Serilog;

namespace PlantBasedPizza.Events;

public class SqsEventSubscriber
{
    private const string TRACEPARENT_STRING = "traceparent";
    private const string DD_TRACE_ID = "ddtraceid";
    private const string DD_SPAN_ID = "ddspanid";
    
    private readonly AmazonSQSClient _sqsClient;

    public SqsEventSubscriber(AmazonSQSClient sqsClient)
    {
        _sqsClient = sqsClient;
    }

    public async Task<string> GetQueueUrl(string queue)
    {
        var queueName = $"{queue}-{Environment.GetEnvironmentVariable("BUILD_VERSION")}";
        
        var describeQueue =
            await this._sqsClient.GetQueueUrlAsync(queueName);

        return describeQueue.QueueUrl;
    }

    public async Task<List<ParseEventResponse<T>>> GetMessages<T>(string queueUrl)
        where T : IntegrationEvent
    {
        var messages = await this._sqsClient.ReceiveMessageAsync(queueUrl);
        
        var response = new List<ParseEventResponse<T>>();

        foreach (var message in messages.Messages)
        {
            var eventBridgeEventWrapper = JsonSerializer.Deserialize<EventBridgeEvent>(message.Body);
            
            var formatter = new JsonEventFormatter<T>();
            var evtWrapper = await formatter.DecodeStructuredModeMessageAsync(new MemoryStream(Encoding.UTF8.GetBytes(eventBridgeEventWrapper.Detail.ToJsonString())), new ContentType("application/json"), new List<CloudEventAttribute>(1)
            {
                CloudEventAttribute.CreateExtension(TRACEPARENT_STRING, CloudEventAttributeType.String)
            });
        
            var traceParent = "";
            
            foreach (var (attribute, value) in evtWrapper.GetPopulatedAttributes())
            {
                if (attribute.Name == TRACEPARENT_STRING)
                {
                    traceParent = value.ToString();
                }
            }

            var evtData = evtWrapper.Data as T;

            response.Add(new ParseEventResponse<T>()
            {
                EventData = evtData,
                TraceParent = traceParent,
                QueueTime = (DateTimeOffset.Now - evtWrapper.Time!.Value).Milliseconds,
                EventId = evtWrapper.Id!,
                MessageId = message.MessageId,
                ReceiptHandle = message.ReceiptHandle
            });
        }

        return response;
    }
    
    public async Task<List<ParseEventResponse<T>>> ParseMessages<T>(List<SQSEvent.SQSMessage> messages)
        where T : IntegrationEvent
    {
        var response = new List<ParseEventResponse<T>>();

        foreach (var message in messages)
        {
            var eventBridgeEventWrapper = JsonSerializer.Deserialize<EventBridgeEvent>(message.Body);
            
            var formatter = new JsonEventFormatter<T>();
            var evtWrapper = await formatter.DecodeStructuredModeMessageAsync(new MemoryStream(Encoding.UTF8.GetBytes(eventBridgeEventWrapper.Detail.ToJsonString())), new ContentType("application/json"), new List<CloudEventAttribute>(1)
            {
                CloudEventAttribute.CreateExtension(TRACEPARENT_STRING, CloudEventAttributeType.String),
                CloudEventAttribute.CreateExtension(DD_TRACE_ID, CloudEventAttributeType.String),
                CloudEventAttribute.CreateExtension(DD_SPAN_ID, CloudEventAttributeType.String),
            });
        
            var traceParent = "";
            ulong traceId = 0;
            ulong spanId = 0;
            
            foreach (var (attribute, value) in evtWrapper.GetPopulatedAttributes())
            {
                if (attribute.Name == TRACEPARENT_STRING)
                {
                    traceParent = value.ToString();
                }
                if (attribute.Name == DD_TRACE_ID)
                {
                    traceId = ulong.Parse(value.ToString());
                }
                if (attribute.Name == DD_SPAN_ID)
                {
                    spanId = ulong.Parse(value.ToString());
                }
            }

            var evtData = evtWrapper.Data as T;

            response.Add(new ParseEventResponse<T>()
            {
                EventData = evtData,
                TraceParent = traceParent,
                QueueTime = (DateTimeOffset.Now - evtWrapper.Time!.Value).Milliseconds,
                EventId = evtWrapper.Id!,
                MessageId = message.MessageId,
                ReceiptHandle = message.ReceiptHandle,
                EventPublishDate = evtWrapper.Time!.Value.LocalDateTime,
                TraceId = traceId,
                SpanId = spanId
            });
        }

        return response;
    }

    public async Task Ack<T>(string queueUrl, ParseEventResponse<T> message)
    {
        await this._sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
    }
}