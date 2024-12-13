using System.Net.Http.Json;
using System.Text.Json;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Payments.InMemoryTests;

public class IntegrationTestDriver : ITestDriver
{
    private readonly HttpClient _client;

    public IntegrationTestDriver()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:8081");
    }
    
    public async Task<HttpResponseMessage> TakePaymentWithValidBody(string orderIdentifier, decimal amount, string? eventId = null)
    {
        if (eventId is null)
            eventId = Guid.NewGuid().ToString();
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"/events/take-payment");
        httpRequestMessage.Content = JsonContent.Create(new
        {
            OrderIdentifier = orderIdentifier,
            PaymentAmount = amount
        });
        httpRequestMessage.Headers.Add(EventConstants.EVENT_ID_HEADER_KEY, eventId);

        await Task.Delay(TimeSpan.FromSeconds(1));

        return await _client.SendAsync(httpRequestMessage);
    }
    
    public async Task<HttpResponseMessage> TakePaymentWithInvalidPaymentAmount(string orderIdentifier, string? eventId = null)
    {
        if (eventId is null)
            eventId = Guid.NewGuid().ToString();
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"/events/take-payment");
        httpRequestMessage.Content = JsonContent.Create(new
        {
            OrderIdentifier = orderIdentifier,
            PaymentAmount = -1
        });
        httpRequestMessage.Headers.Add(EventConstants.EVENT_ID_HEADER_KEY, eventId);

        await Task.Delay(TimeSpan.FromSeconds(1));

        return await _client.SendAsync(httpRequestMessage);
    }
    
    public async Task<HttpResponseMessage> TakePaymentWithInvalidBody(string orderIdentifier, decimal amount, string? eventId = null)
    {
        if (eventId is null)
            eventId = Guid.NewGuid().ToString();
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"/events/take-payment");
        httpRequestMessage.Headers.Add(EventConstants.EVENT_ID_HEADER_KEY, eventId);
        httpRequestMessage.Content = JsonContent.Create(new
        {
            OrderId = orderIdentifier,
            Value = -1
        });

        await Task.Delay(TimeSpan.FromSeconds(1));

        return await _client.SendAsync(httpRequestMessage);
    }

    public async Task<int> VerifySuccessEventReceivedFor(VerificationOptions options)
    {
        var events = await this.GetEventsFor(options.OrderIdentifier);
        
        var successEvent = events.Count(evt => evt.EventName == "PaymentSuccessfulEventV1");

        return successEvent;
    }

    public async Task<int> VerifyFailureEventReceivedFor(VerificationOptions options)
    {
        var events = await this.GetEventsFor(options.OrderIdentifier);
        
        var successEvent = events.Count(evt => evt.EventName == "PaymentFailedEventV1");

        return successEvent;
    }

    public async Task<int> VerifyNoEventIsReceived(string orderIdentifier)
    {
        var events = await this.GetEventsFor(orderIdentifier);

        return events.Count;
    }
    
    private async Task<List<ReceivedEvent>> GetEventsFor(string orderIdentifier)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"/events/{orderIdentifier}");

        var responseMessage = await _client.SendAsync(httpRequestMessage);
        
        return JsonSerializer.Deserialize<List<ReceivedEvent>>(await responseMessage.Content.ReadAsStringAsync())!;
    }
}