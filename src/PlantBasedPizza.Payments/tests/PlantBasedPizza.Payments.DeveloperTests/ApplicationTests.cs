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
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var driver = TestDriverFactory.LoadTestDriver(publisher, memoryCache);

        var orderIdentifier = Guid.NewGuid().ToString();
        await driver.TakePaymentWithValidBody(orderIdentifier, 100);

        var successEventsReceived = await driver.VerifySuccessEventReceivedFor(new VerificationOptions(orderIdentifier));
        
        successEventsReceived.Should().Be(1);
    }
    
    [Fact]
    public async Task TakePaymentRequestIsReceivedTwice_PaymentSuccessfulEventShouldBePublishedOnce()
    {
        var publisher = new InMemoryEventPublisher();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var driver = TestDriverFactory.LoadTestDriver(publisher, memoryCache);
        
        var orderIdentifier = Guid.NewGuid().ToString();
        await driver.TakePaymentWithValidBody(orderIdentifier, 100);
        await driver.TakePaymentWithValidBody(orderIdentifier, 100);

        var successEventsReceived = await driver.VerifySuccessEventReceivedFor(new VerificationOptions(orderIdentifier));
        
        successEventsReceived.Should().Be(1);
    }
    
    [Fact]
    public async Task WhenTheSameEventIdIsReceivedTwice_PaymentSuccessfulEventShouldBePublishedOnce()
    {
        var publisher = new InMemoryEventPublisher();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var driver = TestDriverFactory.LoadTestDriver(publisher, memoryCache);

        var eventId = Guid.NewGuid().ToString();
        var orderIdentifier = Guid.NewGuid().ToString();
        await driver.TakePaymentWithValidBody(orderIdentifier, 100, eventId);
        await driver.TakePaymentWithValidBody(orderIdentifier, 100, eventId);
    
        var successEventsReceived = await driver.VerifySuccessEventReceivedFor(new VerificationOptions(orderIdentifier));
        
        successEventsReceived.Should().Be(1);
    }
    
    [Fact]
    public async Task WhenInvalidEventBodyIsReceived_ShouldReturn400ErrorAndTelemetry()
    {
        var publisher = new InMemoryEventPublisher();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var driver = TestDriverFactory.LoadTestDriver(publisher, memoryCache);

        var eventId = Guid.NewGuid().ToString();
        var orderIdentifier = Guid.NewGuid().ToString();
        await driver.TakePaymentWithInvalidBody(orderIdentifier, 100, eventId);
    
        var failedEventsReceived = await driver.VerifyFailureEventReceivedFor(new VerificationOptions(orderIdentifier, false));
        
        failedEventsReceived.Should().Be(0);
    }
    
    [Fact]
    public async Task WhenNegativePaymentAmountIsReceived_ShouldReturn400ErrorAndTelemetry()
    {
        var publisher = new InMemoryEventPublisher();
        var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        
        var driver = TestDriverFactory.LoadTestDriver(publisher, memoryCache);

        var eventId = Guid.NewGuid().ToString();
        var orderIdentifier = Guid.NewGuid().ToString();
        await driver.TakePaymentWithInvalidPaymentAmount(orderIdentifier, eventId);
    
        var failedEventsReceived = await driver.VerifyFailureEventReceivedFor(new VerificationOptions(orderIdentifier));
        
        failedEventsReceived.Should().Be(1);
    }
}