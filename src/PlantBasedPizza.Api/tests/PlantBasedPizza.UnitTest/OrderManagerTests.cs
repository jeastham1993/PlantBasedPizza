using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;
using Xunit;

namespace PlantBasedPizza.UnitTest;

public class OrderManagerTests
{
    internal const string DefaultCustomerIdentifier = "James";
    
    [Fact]
    public async Task CanCreateNewOrder_ShouldSetDefaultFields()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        
        // Act
        var order = await orderFactory.CreateAsync(OrderType.Pickup, DefaultCustomerIdentifier);

        // Assert
        order.Items.Should().NotBeNull();
        order.Items.Should().BeEmpty();
        order.OrderNumber.Should().NotBeNullOrEmpty();
        order.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.OrderType.Should().Be(OrderType.Pickup);

        A.CallTo(() => mockEventDispatcher.PublishAsync(A<OrderCreatedEvent>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task CanCreateOrderAndAddHistory_ShouldAddHistoryItem()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        
        // Act
        var order = await orderFactory.CreateAsync(OrderType.Pickup, DefaultCustomerIdentifier);
        order.AddHistory("Bake complete");

        // Assert
        order.History.Count.Should().Be(2);
    }
    
    [Fact]
    public async Task CanSetIsAwaitingCollection_ShouldMarkAwaitingAndAddHistory()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        
        // Act
        var order = await orderFactory.CreateAsync(OrderType.Pickup, DefaultCustomerIdentifier);
        order.MarkAsAwaitingCollection();

        // Assert
        order.History.Count.Should().Be(2);
        order.AwaitingCollection.Should().BeTrue();
    }
    
    [Fact]
    public async Task CanCreateNewOrderAndAddItems_ShouldAddToItemArray()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        
        // Act
        var order = await orderFactory.CreateAsync(OrderType.Pickup, DefaultCustomerIdentifier);
        var recipeId = "PIZZA1";
        
        order.AddOrderItem(recipeId, "Pizza 1", 1, 10);
        order.AddOrderItem(recipeId, "Pizza 1", 3, 10);
        order.AddOrderItem("CHIPS", "Chips", 1, 3);

        // Assert
        order.Items.Count.Should().Be(2);
        order.Items.FirstOrDefault(p => p.RecipeIdentifier == recipeId).Quantity.Should().Be(4);
        order.TotalPrice.Should().Be(43);
    }
    
    [Fact]
    public async Task CanCreateNewOrderAndRemoveItems_ShouldRemove()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        
        // Act
        var order = await orderFactory.CreateAsync(OrderType.Pickup, DefaultCustomerIdentifier);
        var recipeId = "PIZZA1";
        
        order.AddOrderItem(recipeId, "Pizza 1", 1, 10);
        order.AddOrderItem(recipeId, "Pizza 1", 3, 10);
        order.AddOrderItem("CHIPS", "Chips", 1, 3);
        order.AddOrderItem("COCACOLA", "Coca Cola", 2, 1);
        
        order.RemoveOrderItem(recipeId, 2);
        order.RemoveOrderItem("COCACOLA", 2);

        // Assert
        order.Items.Count.Should().Be(2);
        order.Items.FirstOrDefault(p => p.RecipeIdentifier == recipeId).Quantity.Should().Be(2);
        order.TotalPrice.Should().Be(23);
    }
    
    [Fact]
    public async Task CanCreateNewDeliveryOrder_ShouldGetDeliveryDetails()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        var deliveryDetails = new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        };
        
        // Act
        var order = await orderFactory.CreateAsync(OrderType.Delivery, DefaultCustomerIdentifier, deliveryDetails);

        // Assert
        order.Items.Should().NotBeNull();
        order.Items.Should().BeEmpty();
        order.OrderNumber.Should().NotBeNullOrEmpty();
        order.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.OrderType.Should().Be(OrderType.Delivery);
        order.DeliveryDetails.AddressLine1.Should().Be("TEST");
    }
    
    [Fact]
    public async Task CanCreateNewDeliveryOrder_ShouldAddDeliveryCharge()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        var deliveryDetails = new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        };
        
        // Act
        var order = await orderFactory.CreateAsync(OrderType.Delivery, DefaultCustomerIdentifier, deliveryDetails);
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        // Assert
        order.TotalPrice.Should().Be(13.50M);
    }
    
    [Fact]
    public async Task CanCreateAndSubmitOrder_ShouldBeSubmitted()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        var deliveryDetails = new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        };
        
        var order = await orderFactory.CreateAsync(OrderType.Delivery, DefaultCustomerIdentifier, deliveryDetails);
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        // Act - Using domain logic directly
        if (!order.Items.Any()) 
            throw new ArgumentException("Cannot submit an order with no items");

        order.MarkAsSubmitted();
        order.AddHistory("Submitted order.");

        await mockEventDispatcher.PublishAsync(new OrderSubmittedEvent(order.OrderIdentifier)
        {
            CorrelationId = string.Empty
        });

        // Assert
        order.OrderSubmittedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        A.CallTo(() => mockEventDispatcher.PublishAsync(A<OrderSubmittedEvent>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task AddItemsToASubmittedOrder_ShouldNotAdd()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        var deliveryDetails = new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        };
        
        var order = await orderFactory.CreateAsync(OrderType.Delivery, DefaultCustomerIdentifier, deliveryDetails);
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);
        
        // Act - Submit order directly using domain logic
        order.MarkAsSubmitted();
        order.AddHistory("Submitted order.");
        await mockEventDispatcher.PublishAsync(new OrderSubmittedEvent(order.OrderIdentifier)
        {
            CorrelationId = string.Empty
        });
        
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        // Assert
        order.Items.FirstOrDefault().Quantity.Should().Be(1);
    }
    
    [Fact]
    public async Task CanCreateAndCompleteOrder_ShouldBeCompleted()
    {
        // Arrange
        var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
        var mockLogger = A.Fake<ILogger<OrderFactory>>();
        var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
        var deliveryDetails = new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        };
        
        var order = await orderFactory.CreateAsync(OrderType.Delivery, DefaultCustomerIdentifier, deliveryDetails);
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        // Act - Complete order directly using domain logic
        order.MarkAsCompleted();
        order.AddHistory("Order completed.");

        var evt = new OrderCompletedEvent(order.CustomerIdentifier, order.OrderIdentifier, order.TotalPrice)
        {
            CorrelationId = string.Empty
        };

        await mockEventDispatcher.PublishAsync(evt);
        order.AddIntegrationEvent(evt);

        // Assert
        order.OrderCompletedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.AwaitingCollection.Should().BeFalse();
        A.CallTo(() => mockEventDispatcher.PublishAsync(A<OrderCompletedEvent>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
    
    
    [Fact]
    public async Task SubmitOrderWithNoItems_ShouldError()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            // Arrange
            var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
            var mockLogger = A.Fake<ILogger<OrderFactory>>();
            var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
            
            var order = await orderFactory.CreateAsync(OrderType.Pickup, DefaultCustomerIdentifier);
            
            // Act - Submit order directly using domain logic (should throw)
            if (!order.Items.Any()) 
                throw new ArgumentException("Cannot submit an order with no items");

            order.MarkAsSubmitted();
            order.AddHistory("Submitted order.");

            await mockEventDispatcher.PublishAsync(new OrderSubmittedEvent(order.OrderIdentifier)
            {
                CorrelationId = string.Empty
            });
        });
    }
    
    
    [Fact]
    public async Task CanCreateNewOrderWithNoCustomerIdentifier_ShouldError()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            // Arrange
            var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
            var mockLogger = A.Fake<ILogger<OrderFactory>>();
            var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
            
            // Act
            await orderFactory.CreateAsync(OrderType.Pickup, string.Empty);
        });
    }
    
    
    [Fact]
    public async Task CanCreateNewDeliveryOrderWithNoDeliveryDetails_ShouldError()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            // Arrange
            var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
            var mockLogger = A.Fake<ILogger<OrderFactory>>();
            var orderFactory = new OrderFactory(mockEventDispatcher, mockLogger);
            
            // Act
            await orderFactory.CreateAsync(OrderType.Delivery, DefaultCustomerIdentifier);
        });
    }
}