using System.Text.Json.Serialization;
using Dapr.Client;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public record TakePaymentRequest
{
    [JsonPropertyName("OrderIdentifier")]
    public string OrderIdentifier { get; set; }
    
    [JsonPropertyName("PaymentAmount")]
    public decimal PaymentAmount { get; set; }
}


public record RefundPaymentRequest
{
    [JsonPropertyName("OrderIdentifier")]
    public string OrderIdentifier { get; set; }
    
    [JsonPropertyName("PaymentAmount")]
    public decimal PaymentAmount { get; set; }
}

public class PaymentService(DaprClient daprClient) : IPaymentService
{
    private const string SOURCE = "orders";
    
    public async Task TakePayment(string orderIdentifier, decimal paymentAmount)
    {
        var eventType = $"payments.takepayment.v1";
        
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType},
            { EventConstants.EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() }
        };
        
        await daprClient.PublishEventAsync("payments", eventType, new TakePaymentRequest()
        {
            OrderIdentifier = orderIdentifier,
            PaymentAmount = paymentAmount
        }, eventMetadata);
    }

    public async Task RefundPayment(string orderIdentifier, decimal paymentAmount)
    {
        var eventType = $"payments.refundpayment.v1";
        
        var eventMetadata = new Dictionary<string, string>(3)
        {
            { EventConstants.EVENT_SOURCE_HEADER_KEY, SOURCE },
            { EventConstants.EVENT_TYPE_HEADER_KEY, eventType},
            { EventConstants.EVENT_ID_HEADER_KEY, Guid.NewGuid().ToString() }
        };
        
        await daprClient.PublishEventAsync("payments", eventType, new RefundPaymentRequest()
        {
            OrderIdentifier = orderIdentifier,
            PaymentAmount = paymentAmount
        }, eventMetadata);
    }
}