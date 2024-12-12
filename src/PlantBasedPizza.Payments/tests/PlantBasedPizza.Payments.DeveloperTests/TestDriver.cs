using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PlantBasedPizza.Events;
using PlantBasedPizza.Payments.TakePayment;

namespace PlantBasedPizza.Payments.InMemoryTests;

public class TestDriver(WebApplicationFactory<Program> application)
{
    private HttpClient _client = application.CreateClient();
    
    public async Task<HttpResponseMessage> TakePayment(string orderIdentifier, decimal amount, string? eventId = null)
    {
        if (eventId is null)
            eventId = Guid.NewGuid().ToString();
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/take-payment");
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(new TakePaymentCommand()
            {
                OrderIdentifier = orderIdentifier,
                PaymentAmount = amount
            }), Encoding.UTF8,
            "application/json");
        httpRequestMessage.Headers.Add(EventConstants.EVENT_ID_HEADER_KEY, eventId);

        return await _client.SendAsync(httpRequestMessage);
    }
    
    public async Task<HttpResponseMessage> TakePaymentWithInvalidBody(string orderIdentifier, decimal amount, string? eventId = null)
    {
        if (eventId is null)
            eventId = Guid.NewGuid().ToString();
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/take-payment");
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(new {
                OrderId = "ORD123",
                Value = 100
            }), Encoding.UTF8,
            "application/json");
        httpRequestMessage.Headers.Add(EventConstants.EVENT_ID_HEADER_KEY, eventId);

        return await _client.SendAsync(httpRequestMessage);
    }
}