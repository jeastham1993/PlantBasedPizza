using System;
using System.Linq;
using FluentAssertions;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entities;
using PlantBasedPizza.Shared.Events;
using Xunit;

namespace PlantBasedPizza.UnitTest;

public class OrderManagerTests
{
    internal const string DefaultCustomerIdentifier = "James";
    internal const string DefaultOrderIdentifier = "MYTESTORDER";
    
    [Fact]
    public void CanCreateNewOrder_ShouldSetDefaultFields()
    {
        string? createdOrder = null;
        
        DomainEvents.Register<OrderCreatedEvent>((evt) =>
        {
            createdOrder = evt.OrderIdentifier;
        });
        
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Pickup, DefaultCustomerIdentifier);

        order.Items.Should().NotBeNull();
        order.Items.Should().BeEmpty();
        order.OrderNumber.Should().NotBeNullOrEmpty();
        order.OrderDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        order.OrderType.Should().Be(OrderType.Pickup);

        createdOrder.Should().NotBeNull();
    }
    
    [Fact]
    public void CanCreateOrderAndAddHistory_ShouldAddHistoryItem()
    {
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Pickup, DefaultCustomerIdentifier);

        order.AddHistory("Bake complete");

        order.History().Count.Should().Be(2);
    }
    
    [Fact]
    public void CanSetIsAwaitingCollection_ShouldMarkAwaitingAndAddHistory()
    {
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Pickup, DefaultCustomerIdentifier);

        order.IsAwaitingCollection();

        order.History().Count.Should().Be(2);
        order.AwaitingCollection.Should().BeTrue();
    }
    
    [Fact]
    public void CanCreateNewOrderAndAddItems_ShouldAddToItemArray()
    {
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Pickup, DefaultCustomerIdentifier);

        var recipeId = "PIZZA1";
        
        order.AddOrderItem(recipeId, "Pizza 1", 1, 10);
        order.AddOrderItem(recipeId, "Pizza 1", 3, 10);
        order.AddOrderItem("CHIPS", "Chips", 1, 3);

        order.Items.Count.Should().Be(2);
        order.Items.FirstOrDefault(p => p.RecipeIdentifier == recipeId).Quantity.Should().Be(4);
        order.TotalPrice.Should().Be(43);
    }
    
    [Fact]
    public void CanCreateNewOrderAndRemoveItems_ShouldRemove()
    {
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Pickup, DefaultCustomerIdentifier);

        var recipeId = "PIZZA1";
        
        order.AddOrderItem(recipeId, "Pizza 1", 1, 10);
        order.AddOrderItem(recipeId, "Pizza 1", 3, 10);
        order.AddOrderItem("CHIPS", "Chips", 1, 3);
        order.AddOrderItem("COCACOLA", "Coca Cola", 2, 1);
        
        order.RemoveOrderItem(recipeId, 2);
        order.RemoveOrderItem("COCACOLA", 2);

        order.Items.Count.Should().Be(2);
        order.Items.FirstOrDefault(p => p.RecipeIdentifier == recipeId).Quantity.Should().Be(2);
        order.TotalPrice.Should().Be(23);
    }
    
    [Fact]
    public void CanCreateNewDeliveryOrder_ShouldGetDeliveryDetails()
    {
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Delivery, DefaultCustomerIdentifier, new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        });

        order.Items.Should().NotBeNull();
        order.Items.Should().BeEmpty();
        order.OrderNumber.Should().NotBeNullOrEmpty();
        order.OrderDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        order.OrderType.Should().Be(OrderType.Delivery);
        order.DeliveryDetails.AddressLine1.Should().Be("TEST");
    }
    
    [Fact]
    public void CanCreateNewDeliveryOrder_ShouldAddDeliveryCharge()
    {
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Delivery, DefaultCustomerIdentifier, new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        });
        
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        order.TotalPrice.Should().Be(13.50M);
    }
    
    [Fact]
    public void CanCreateAndSubmitOrder_ShouldBeSubmitted()
    {
        string submittedOrder = null;
        
        DomainEvents.Register<OrderSubmittedEvent>((evt) =>
        {
            submittedOrder = evt.OrderIdentifier;
            evt.EventDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        });
        
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Delivery, DefaultCustomerIdentifier, new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        });
        
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        order.SubmitOrder();

        order.OrderSubmittedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        submittedOrder.Should().NotBeNull();
    }
    
    [Fact]
    public void AddItemsToASubmittedOrder_ShouldNotAdd()
    {
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Delivery, DefaultCustomerIdentifier, new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        });
        
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        order.SubmitOrder();
        
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        order.Items.FirstOrDefault().Quantity.Should().Be(1);
    }
    
    [Fact]
    public void CanCreateAndCompletetOrder_ShouldBeCompleted()
    {
        string completedOrder = null;
        
        DomainEvents.Register<OrderCompletedEvent>((evt) =>
        {
            completedOrder = evt.OrderIdentifier;
        });
        
        var order = Order.Create(DefaultOrderIdentifier, OrderType.Delivery, DefaultCustomerIdentifier, new DeliveryDetails()
        {
            AddressLine1 = "TEST",
            Postcode = "XN6 7UY"
        });
        
        order.AddOrderItem("PIZZA", "Pizza 1", 1, 10);

        order.CompleteOrder();

        order.OrderCompletedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        order.AwaitingCollection.Should().BeFalse();

        completedOrder.Should().NotBeNull();
    }
    
    
    [Fact]
    public void SubmitOrderWithNoItems_ShouldError()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var order = Order.Create(DefaultOrderIdentifier, OrderType.Pickup, DefaultCustomerIdentifier);
            
            order.SubmitOrder();
        });
    }
    
    
    [Fact]
    public void CanCreateNewOrderWithNoCustomerIdentifier_ShouldError()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            Order.Create(DefaultOrderIdentifier, OrderType.Pickup, string.Empty);
        });
    }
    
    
    [Fact]
    public void CanCreateNewDeliveryOrderWithNoDeliveryDetails_ShouldError()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Order.Create(DefaultOrderIdentifier, OrderType.Delivery, DefaultCustomerIdentifier);
        });
    }
}