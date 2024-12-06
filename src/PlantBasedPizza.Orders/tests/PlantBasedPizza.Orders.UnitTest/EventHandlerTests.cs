using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NJsonSchema;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.PaymentSuccess;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Orders.Worker;
using PlantBasedPizza.Orders.Worker.IntegrationEvents;

namespace PlantBasedPizza.Orders.UnitTest;

public class EventHandlerTests
{
    private readonly Mock<IUserNotificationService> userNotificationService;
    private readonly Mock<IWorkflowEngine> workflowEngine;
    private readonly Mock<IFeatures> features;
    private readonly Mock<Idempotency> idempotency;

    public EventHandlerTests()
    {
        userNotificationService = new Mock<IUserNotificationService>();
        workflowEngine = new Mock<IWorkflowEngine>();
        features = new Mock<IFeatures>();
        idempotency = new Mock<Idempotency>();
    }
    
    [Fact]
    public async Task HandlePaymentSuccessEvent_WhenEventStructureIsCorrectShouldSubmitOrder()
    {
        var testOrder = Order.Create(OrderType.Pickup, "testuser");
        
        var inMemoryOrderRepository = new InMemoryOrderRepository();
        await inMemoryOrderRepository.Add(testOrder);

        var paymentSuccessEventHandler = new PaymentSuccessEventHandler(inMemoryOrderRepository,
            userNotificationService.Object, workflowEngine.Object, features.Object);

        var expectedPaymentSuccessSchema =
            await JsonSchema.FromJsonAsync(
                await File.ReadAllTextAsync("expected_schemas/paymentSuccessfulEvent.v1.json"));

        var sampleEvent = expectedPaymentSuccessSchema.ToSampleJson();
        var paymentSuccessEvent = JsonSerializer.Deserialize<PaymentSuccessfulEventV1>(sampleEvent.ToString()!);
        
        await EventHandlers.HandlePaymentSuccessfulEvent(
            paymentSuccessEventHandler,
            idempotency.Object,
            new DefaultHttpContext(),
            paymentSuccessEvent
        );
        
        var order = await inMemoryOrderRepository.Retrieve(paymentSuccessEvent.OrderIdentifier);

        order.Events.FirstOrDefault(evt => evt.EventName == "order.orderConfirmed" && evt.EventVersion == "v1").Should()
            .NotBeNull();
    }
    
    [Fact]
    public async Task HandleInvalidPaymentSuccessEvent_WhenEventStructureIsInvalidShouldFail()
    {
        var testOrder = Order.Create(OrderType.Pickup, "testuser");
        
        var inMemoryOrderRepository = new InMemoryOrderRepository(true);
        await inMemoryOrderRepository.Add(testOrder);

        var paymentSuccessEventHandler = new PaymentSuccessEventHandler(inMemoryOrderRepository,
            userNotificationService.Object, workflowEngine.Object, features.Object);

        var expectedPaymentSuccessSchema =
            await JsonSchema.FromJsonAsync(
                await File.ReadAllTextAsync("expected_schemas/invalidPaymentSuccessfulEvent.v1.json"));

        var sampleEvent = expectedPaymentSuccessSchema.ToSampleJson();
        var paymentSuccessEvent = JsonSerializer.Deserialize<PaymentSuccessfulEventV1>(sampleEvent.ToString()!);
        
        var handlerResult = await EventHandlers.HandlePaymentSuccessfulEvent(
            paymentSuccessEventHandler,
            idempotency.Object,
            new DefaultHttpContext(),
            paymentSuccessEvent
        );
        
        handlerResult.Should().BeOfType<InternalServerError>();
    }
}