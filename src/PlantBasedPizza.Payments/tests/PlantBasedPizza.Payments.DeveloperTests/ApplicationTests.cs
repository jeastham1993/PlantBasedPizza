using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace PlantBasedPizza.Payments.InMemoryTests;

public class ApplicationTests
{
    [Fact]
    public async Task TakePaymentRequestIsReceived_PaymentSuccessfulEventShouldBePublished()
    {
        var publisher = new InMemoryEventPublisher();
        var exportedItems = new List<Activity>();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var application = Setup.StartInMemoryServer(publisher, memoryCache, exportedItems);
        var testDriver = new TestDriver(application);
        
        await testDriver.TakePayment("ORD123", 100);

        publisher.SuccessEvents.Count.Should().Be(1);
        var cachedPayment = await memoryCache.GetStringAsync("ORD123");
        cachedPayment.Should().Be("processed");
        
        var httpSpan = exportedItems.First(span => span.DisplayName == "POST /take-payment");
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "orderIdentifier").Value.Should().Be("ORD123").Should()
            .NotBeNull("The order identifier should be added to telemetry");
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "paymentAmount").Value.Should().Be("100.00").Should()
            .NotBeNull("The payment amount should be added to telemetry");
        
        exportedItems.FirstOrDefault(activity => activity.DisplayName == $"publish payments.paymentSuccessful.v1").Should().NotBeNull();
    }
    
    [Fact]
    public async Task TakePaymentRequestIsReceivedTwice_PaymentSuccessfulEventShouldBePublishedOnce()
    {
        var publisher = new InMemoryEventPublisher();
        var exportedItems = new List<Activity>();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var application = Setup.StartInMemoryServer(publisher, memoryCache, exportedItems);
        var testDriver = new TestDriver(application);

        await testDriver.TakePayment("ORD123", 100);
        await testDriver.TakePayment("ORD123", 100);

        publisher.SuccessEvents.Count.Should().Be(1);
        
        exportedItems.Count(span => span.DisplayName == "POST /take-payment").Should().Be(2, "Payment request is received twice");
        
        var httpSpan = exportedItems.First(span => span.DisplayName == "POST /take-payment");
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "orderIdentifier").Value.Should().Be("ORD123").Should()
            .NotBeNull("The order identifier should be added to the traces");
        
        var secondHttpSpan = exportedItems.Last(span => span.DisplayName == "POST /take-payment");
        secondHttpSpan.Tags.FirstOrDefault(tag => tag.Key == "payment-processed").Value.Should().Be("true");
    }
    
    [Fact]
    public async Task WhenTheSameEventIdIsReceivedTwice_PaymentSuccessfulEventShouldBePublishedOnce()
    {
        var publisher = new InMemoryEventPublisher();
        var exportedItems = new List<Activity>();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var application = Setup.StartInMemoryServer(publisher, memoryCache, exportedItems);
        var testDriver = new TestDriver(application);
        
        var eventId = Guid.NewGuid().ToString();
        
        await testDriver.TakePayment("ORD123", 100, eventId);
        await testDriver.TakePayment("ORD123", 100, eventId);

        publisher.SuccessEvents.Count.Should().Be(1);
        
        exportedItems.Count(span => span.DisplayName == "POST /take-payment").Should().Be(2, "Payment request is received twice");
        
        var httpSpan = exportedItems.First(span => span.DisplayName == "POST /take-payment");
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "orderIdentifier").Value.Should().Be("ORD123").Should()
            .NotBeNull("The order identifier should be added to the traces");
        
        var secondHttpSpan = exportedItems.Last(span => span.DisplayName == "POST /take-payment");
        secondHttpSpan.Tags.FirstOrDefault(tag => tag.Key == "events.idempotent").Value.Should().Be("true");
    }
    
    [Fact]
    public async Task WhenSuccessEventFailsToPublish_PaymentFailedEventShouldBePublished()
    {
        var publisher = new InMemoryEventPublisher(true);
        var exportedItems = new List<Activity>();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var application = Setup.StartInMemoryServer(publisher, memoryCache, exportedItems);
        var testDriver = new TestDriver(application);
        
        var eventId = Guid.NewGuid().ToString();
        var httpResult = await testDriver.TakePayment("ORD123", 100, eventId);

        httpResult.IsSuccessStatusCode.Should().BeFalse();
        publisher.SuccessEvents.Count.Should().Be(0);
        publisher.FailedEvents.Count.Should().Be(1);
        
        exportedItems.Count(span => span.DisplayName == "POST /take-payment").Should().Be(1, "Payment request is received twice");
        
        var httpSpan = exportedItems.First(span => span.DisplayName == "POST /take-payment");
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "orderIdentifier").Value.Should().Be("ORD123").Should()
            .NotBeNull("The order identifier should be added to the traces");
    }
  
    [Fact]
    public async Task WhenInvalidEventBodyIsReceived_ShouldReturn400ErrorAndTelemetry()
    {
        var publisher = new InMemoryEventPublisher(true);
        var exportedItems = new List<Activity>();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var application = Setup.StartInMemoryServer(publisher, memoryCache, exportedItems);
        var testDriver = new TestDriver(application);
        
        var httpResult = await testDriver.TakePaymentWithInvalidBody("ORD123", 100);

        httpResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        publisher.SuccessEvents.Count.Should().Be(0);
        publisher.FailedEvents.Count.Should().Be(0);
        
        exportedItems.Count(span => span.DisplayName == "POST /take-payment").Should().Be(1);
        
        var httpSpan = exportedItems.First(span => span.DisplayName == "POST /take-payment");
        
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "orderIdentifier").Value.Should().Be("null").Should()
            .NotBeNull("The order identifier should be added to telemetry");
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "paymentAmount").Value.Should().Be("0.00").Should()
            .NotBeNull("The payment amount should be added to telemetry");
    }
  
    [Fact]
    public async Task WhenNegativePaymentAmountIsReceived_ShouldReturn400ErrorAndTelemetry()
    {
        var publisher = new InMemoryEventPublisher(true);
        var exportedItems = new List<Activity>();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var application = Setup.StartInMemoryServer(publisher, memoryCache, exportedItems);
        var testDriver = new TestDriver(application);
        
        var httpResult = await testDriver.TakePayment("ORD123", -1);

        httpResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        publisher.SuccessEvents.Count.Should().Be(0);
        publisher.FailedEvents.Count.Should().Be(1);
        
        exportedItems.Count(span => span.DisplayName == "POST /take-payment").Should().Be(1);
        
        var httpSpan = exportedItems.First(span => span.DisplayName == "POST /take-payment");
        
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "orderIdentifier").Value.Should().Be("ORD123").Should()
            .NotBeNull("The order identifier should be added to telemetry");
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "paymentAmount").Value.Should().Be("-1.00").Should()
            .NotBeNull("The payment amount should be added to telemetry");
        httpSpan.Tags.FirstOrDefault(tag => tag.Key == "command.invalid").Value.Should().Be("true").Should()
            .NotBeNull();
    }
}