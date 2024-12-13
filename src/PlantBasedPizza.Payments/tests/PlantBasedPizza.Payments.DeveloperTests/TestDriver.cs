using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using PlantBasedPizza.Events;
using PlantBasedPizza.Payments.TakePayment;

namespace PlantBasedPizza.Payments.InMemoryTests;

public class TestDriver : ITestDriver
{
    private readonly InMemoryEventPublisher _eventPublisher;
    private readonly MemoryDistributedCache _distributedCache;
    private readonly HttpClient _client;

    public List<Activity> Activities { get; set; }

    public TestDriver(InMemoryEventPublisher eventPublisher, MemoryDistributedCache distributedCache)
    {
        _eventPublisher = eventPublisher;
        _distributedCache = distributedCache;
        Activities = new List<Activity>();

        var inMemoryServer = Setup.StartInMemoryServer(eventPublisher, distributedCache, Activities);
        _client = inMemoryServer.CreateClient();
    }

    public async Task<HttpResponseMessage> TakePaymentWithInvalidBody(string orderIdentifier, decimal amount,
        string? eventId = null)
    {
        if (eventId is null)
            eventId = Guid.NewGuid().ToString();

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/take-payment");
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(new
            {
                OrderId = orderIdentifier,
                Value = 100
            }), Encoding.UTF8,
            "application/json");
        httpRequestMessage.Headers.Add(EventConstants.EVENT_ID_HEADER_KEY, eventId);

        return await _client.SendAsync(httpRequestMessage);
    }

    public async Task<HttpResponseMessage> TakePaymentWithInvalidPaymentAmount(string orderIdentifier,
        string? eventId = null)
    {
        if (eventId is null)
            eventId = Guid.NewGuid().ToString();

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/take-payment");
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(new
            {
                orderIdentifier,
                Value = -1
            }), Encoding.UTF8,
            "application/json");
        httpRequestMessage.Headers.Add(EventConstants.EVENT_ID_HEADER_KEY, eventId);

        return await _client.SendAsync(httpRequestMessage);
    }

    public async Task<HttpResponseMessage> TakePaymentWithValidBody(string orderIdentifier, decimal amount,
        string? eventId = null)
    {
        if (eventId is null)
            eventId = Guid.NewGuid().ToString();

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/take-payment");
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(new TakePaymentCommand
            {
                OrderIdentifier = orderIdentifier,
                PaymentAmount = amount
            }), Encoding.UTF8,
            "application/json");
        httpRequestMessage.Headers.Add(EventConstants.EVENT_ID_HEADER_KEY, eventId);

        return await _client.SendAsync(httpRequestMessage);
    }

    public async Task<int> VerifySuccessEventReceivedFor(VerificationOptions options)
    {
        var cachedPayment = await _distributedCache.GetStringAsync(options.OrderIdentifier);
        cachedPayment.Should().Be("processed");

        if (options.VerifyTelemetry)
        {
            var messagingSpan = Activities.First(span => span.DisplayName == "process payments.takepayment.v1");
            messagingSpan.Tags.FirstOrDefault(tag => tag.Key == "orderIdentifier").Value.Should().Be(options.OrderIdentifier)
                .Should()
                .NotBeNull("The order identifier should be added to telemetry");
            messagingSpan.Tags.FirstOrDefault(tag => tag.Key == "paymentAmount").Value.Should().Be("100.00").Should()
                .NotBeNull("The payment amount should be added to telemetry");

            Activities.Find(activity => activity.DisplayName == "publish payments.paymentSuccessful.v1").Should()
                .NotBeNull();
        }

        return _eventPublisher.SuccessEvents.Count;
    }

    public async Task<int> VerifyFailureEventReceivedFor(VerificationOptions options)
    {
        Activities.Count(span => span.DisplayName == "process payments.takepayment.v1").Should()
            .Be(1, "Payment request is received");

        if (options.VerifyTelemetry)
        {
            var httpSpan = Activities.First(span => span.DisplayName == "process payments.takepayment.v1");
            httpSpan.Tags.FirstOrDefault(tag => tag.Key == "orderIdentifier").Value.Should().Be(options.OrderIdentifier)
                .Should()
                .NotBeNull("The order identifier should be added to the traces");
        }

        return _eventPublisher.FailedEvents.Count;
    }
}