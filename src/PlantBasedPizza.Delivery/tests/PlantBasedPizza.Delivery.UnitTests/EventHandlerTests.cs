using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.OrderReadyForDelivery;
using PlantBasedPizza.Delivery.Worker;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Delivery.UnitTests;

public class EventHandlerTests
{
    [Fact]
    public async Task ForValidEvent_ShouldStoreInDatabase()
    {
        var repository = new Mock<IDeliveryRequestRepository>();
        repository.Setup(p => p.GetDeliveryStatusForOrder(It.IsAny<string>()))
            .ReturnsAsync((DeliveryRequest?)null);
        repository.Setup(repo => repo.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>(), It.IsAny<List<IntegrationEvent>>())).Verifiable();
        
        var orderReadyForDeliveryHandler = new OrderReadyForDeliveryEventHandler(repository.Object, new Mock<ILogger<OrderReadyForDeliveryEventHandler>>().Object);
        var idempotency = new InMemoryIdempotency();
        
        var eventHandlerResult = await EventHandlers.HandleOrderReadyForDeliveryEvent(
            orderReadyForDeliveryHandler,
            idempotency,
            generateDefaultHttpContext(Guid.NewGuid().ToString()),
            new OrderReadyForDeliveryEventV1()
            {
                OrderIdentifier = "ORD123"
            });
        
        eventHandlerResult.Should().Be(Results.Ok());
        repository.Verify(repo => repo.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>(), It.IsAny<List<IntegrationEvent>>()), Times.Once);
        idempotency.HandledEvents.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task WhenEventIsForAnExistingOrder_ShouldReturnSuccessButNotStoreInDatabase()
    {
        var repository = new Mock<IDeliveryRequestRepository>();
        repository.Setup(p => p.GetDeliveryStatusForOrder(It.IsAny<string>()))
            .ReturnsAsync(new DeliveryRequest("ORD123", new Address("add1", "BT67YU")));
        repository.Setup(repo => repo.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>(), It.IsAny<List<IntegrationEvent>>())).Verifiable();
        
        var orderReadyForDeliveryHandler = new OrderReadyForDeliveryEventHandler(repository.Object, new Mock<ILogger<OrderReadyForDeliveryEventHandler>>().Object);
        var idempotency = new InMemoryIdempotency();
        
        var eventHandlerResult = await EventHandlers.HandleOrderReadyForDeliveryEvent(
            orderReadyForDeliveryHandler,
            idempotency,
            new DefaultHttpContext(),
            new OrderReadyForDeliveryEventV1()
            {
                OrderIdentifier = "ORD123"
            });
        
        eventHandlerResult.Should().Be(Results.Ok());
        repository.Verify(repo => repo.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>(), It.IsAny<List<IntegrationEvent>>()), Times.Never);
        idempotency.HandledEvents.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task WhenEventIsReceivedTwice_ShouldSkipProcessingSecondEvent()
    {
        var repository = new Mock<IDeliveryRequestRepository>();
        repository.Setup(p => p.GetDeliveryStatusForOrder(It.IsAny<string>()))
            .ReturnsAsync((DeliveryRequest?)null);
        repository.Setup(repo => repo.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>(), It.IsAny<List<IntegrationEvent>>())).Verifiable();
        
        var orderReadyForDeliveryHandler = new OrderReadyForDeliveryEventHandler(repository.Object, new Mock<ILogger<OrderReadyForDeliveryEventHandler>>().Object);
        var idempotency = new InMemoryIdempotency();
        var eventId = Guid.NewGuid().ToString();
        
        var firstHandlerResult = await EventHandlers.HandleOrderReadyForDeliveryEvent(
            orderReadyForDeliveryHandler,
            idempotency,
            generateDefaultHttpContext(eventId),
            new OrderReadyForDeliveryEventV1()
            {
                OrderIdentifier = "ORD123"
            });
        
        var secondHandlerResult = await EventHandlers.HandleOrderReadyForDeliveryEvent(
            orderReadyForDeliveryHandler,
            idempotency,
            generateDefaultHttpContext(eventId),
            new OrderReadyForDeliveryEventV1()
            {
                OrderIdentifier = "ORD123"
            });
        
        firstHandlerResult.Should().Be(Results.Ok());
        secondHandlerResult.Should().Be(Results.Ok());
        repository.Verify(repo => repo.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>(), It.IsAny<List<IntegrationEvent>>()), Times.Once);
        idempotency.HandledEvents.Should().HaveCount(1);
    }

    private static HttpContext generateDefaultHttpContext(string eventId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("Cloudevent.id", eventId);
        
        return httpContext;
    }
}