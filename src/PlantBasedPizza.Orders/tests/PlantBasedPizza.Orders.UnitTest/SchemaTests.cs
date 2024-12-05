using FluentAssertions;
using NJsonSchema;
using NJsonSchema.Validation;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.OrderManager.Core.PublicEvents;
using PlantBasedPizza.OrderManager.Core.SubmitOrder;

namespace PlantBasedPizza.Orders.UnitTest;

public class SchemaTests
{
    [Fact]
    public async Task WhenOrderCreated_V1SchemaShouldMatchDefinition()
    {
        var order = Order.Create(OrderType.Pickup, "testuser");

        var evt = order.Events.FirstOrDefault(evt => evt.EventName == "order.orderCreated" && evt.EventVersion == "v1");
        evt.Should().NotBeNull("Should contain order created event");

        var eventJson = evt!.AsString();
        var expectedSchema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync("./expected_schemas/orderCreated.v1.json"));
        var validationResults = expectedSchema.Validate(eventJson);
        validationResults.Count.Should().Be(0, "Should match expected schema");
    }
    [Fact]
    public async Task WhenOrderCreated_V2SchemaShouldMatchDefinition()
    {
        var order = Order.Create(OrderType.Pickup, "testuser");

        var evt = order.Events.FirstOrDefault(evt => evt.EventName == "order.orderCreated" && evt.EventVersion == "v2");
        evt.Should().NotBeNull("Should contain order created event");

        var eventJson = evt!.AsString();
        var expectedSchema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync("./expected_schemas/orderCreated.v2.json"));
        var validationResults = expectedSchema.Validate(eventJson);
        validationResults.Count.Should().Be(0, "Should match expected schema");
    }
    
    [Fact]
    public async Task WhenOrderSubmitted_V1SchemaShouldMatchDefinition()
    {
        var order = Order.Create(OrderType.Pickup, "testuser");
        order.AddOrderItem("marg", "marg", 1, 10);
        order.SubmitOrder();

        var evt = order.Events.FirstOrDefault(evt => evt.EventName == "order.orderSubmitted" && evt.EventVersion == "v1");
        evt.Should().NotBeNull("Should contain order submitted event");

        var eventJson = evt!.AsString();
        var expectedSchema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync("./expected_schemas/orderSubmitted.v1.json"));
        var validationResults = expectedSchema.Validate(eventJson);
        validationResults.Count.Should().Be(0, "Should match expected schema");
    }
    
    [Fact]
    public async Task WhenOrderCancelled_V1SchemaShouldMatchDefinition()
    {
        var order = Order.Create(OrderType.Pickup, "testuser");
        order.AddOrderItem("marg", "marg", 1, 10);
        order.CancelOrder();

        var evt = order.Events.FirstOrDefault(evt => evt.EventName == "order.orderCancelled" && evt.EventVersion == "v1");
        evt.Should().NotBeNull("Should contain order submitted event");

        var eventJson = evt!.AsString();
        var expectedSchema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync("./expected_schemas/orderCancelled.v1.json"));
        var validationResults = expectedSchema.Validate(eventJson);
        validationResults.Count.Should().Be(0, "Should match expected schema");
    }
    
    [Fact]
    public async Task WhenOrderCompleted_V1SchemaShouldMatchDefinition()
    {
        var order = Order.Create(OrderType.Pickup, "testuser");
        order.AddOrderItem("marg", "marg", 1, 10);
        order.SubmitOrder();
        order.CompleteOrder();

        var evt = order.Events.FirstOrDefault(evt => evt.EventName == "order.orderCompleted" && evt.EventVersion == "v1");
        evt.Should().NotBeNull("Should contain order submitted event");

        var eventJson = evt!.AsString();
        var expectedSchema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync("./expected_schemas/orderCompleted.v1.json"));
        var validationResults = expectedSchema.Validate(eventJson);
        validationResults.Count.Should().Be(0, "Should match expected schema");
    }
    
    [Fact]
    public async Task WhenOrderCompleted_V2SchemaShouldMatchDefinition()
    {
        var order = Order.Create(OrderType.Pickup, "testuser");
        order.AddOrderItem("marg", "marg", 1, 10);
        order.SubmitOrder();
        order.CompleteOrder();

        var evt = order.Events.FirstOrDefault(evt => evt.EventName == "order.orderCompleted" && evt.EventVersion == "v2");
        evt.Should().NotBeNull("Should contain order submitted event");

        var eventJson = evt!.AsString();
        var expectedSchema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync("./expected_schemas/orderCompleted.v2.json"));
        var validationResults = expectedSchema.Validate(eventJson);
        validationResults.Count.Should().Be(0, "Should match expected schema");
    }
    
    [Fact]
    public async Task WhenOrderConfirmed_V1SchemaShouldMatchDefinition()
    {
        var order = Order.Create(OrderType.Pickup, "testuser");
        order.AddOrderItem("marg", "marg", 1, 10);
        order.SubmitOrder();
        order.Confirm(10);

        var evt = order.Events.FirstOrDefault(evt => evt.EventName == "order.orderConfirmed" && evt.EventVersion == "v1");
        evt.Should().NotBeNull("Should contain order confirmed event");

        var eventJson = evt!.AsString();
        var expectedSchema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync("./expected_schemas/orderConfirmed.v1.json"));
        var validationResults = expectedSchema.Validate(eventJson);
        validationResults.Count.Should().Be(0, "Should match expected schema");
    }
    
    [Fact]
    public async Task WhenOrderReadyForDelivery_V1SchemaShouldMatchDefinition()
    {
        var order = Order.Create(OrderType.Delivery, "testuser", new DeliveryDetails()
        {
            AddressLine1 = "test",
            AddressLine2 = "test",
            AddressLine3 = "test",
            AddressLine4 = "test",
            AddressLine5 = "test",
            Postcode = "test"
        });
        order.AddOrderItem("marg", "marg", 1, 10);
        order.SubmitOrder();
        order.Confirm(10);
        order.ReadyForDelivery();

        var evt = order.Events.FirstOrDefault(evt => evt.EventName == "order.readyForDelivery" && evt.EventVersion == "v1");
        evt.Should().NotBeNull("Should contain order ready for delivery event");

        var eventJson = evt!.AsString();
        var expectedSchema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync("./expected_schemas/readyForDelivery.v1.json"));
        var validationResults = expectedSchema.Validate(eventJson);
        validationResults.Count.Should().Be(0, "Should match expected schema");
    }
}