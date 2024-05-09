using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Amazon.SQS;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

namespace PlantBasedPizza.Events;

public class SqsEventSubscriber
{
    private readonly AmazonSQSClient _sqsClient;

    public SqsEventSubscriber(AmazonSQSClient sqsClient)
    {
        _sqsClient = sqsClient;
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
                CloudEventAttribute.CreateExtension("traceparent", CloudEventAttributeType.String)
            });
        
            var traceParent = "";
            
            foreach (var (attribute, value) in evtWrapper.GetPopulatedAttributes())
            {
                if (attribute.Name == "traceparent")
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

    public async Task Ack<T>(string queueUrl, ParseEventResponse<T> message)
    {
        await this._sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
    }
}