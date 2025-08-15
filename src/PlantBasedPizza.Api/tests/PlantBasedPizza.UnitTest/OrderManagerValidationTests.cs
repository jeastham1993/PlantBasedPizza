using System.Threading.Tasks;
using FluentAssertions;
using PlantBasedPizza.OrderManager.Core.AddItemToOrder;
using PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;
using PlantBasedPizza.OrderManager.Core.CreatePickupOrder;
using Xunit;

namespace PlantBasedPizza.UnitTest;

public class OrderManagerValidationTests
{
    [Fact]
    public async Task CreatePickupOrderCommandValidator_ShouldAcceptValidRequest()
    {
        // Arrange
        var validator = new CreatePickupOrderCommandValidator();
        var command = new CreatePickupOrderCommand
        {
            CustomerIdentifier = "customer123"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePickupOrderCommandValidator_ShouldRejectEmptyCustomerIdentifier()
    {
        // Arrange
        var validator = new CreatePickupOrderCommandValidator();
        var command = new CreatePickupOrderCommand
        {
            CustomerIdentifier = ""
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Customer identifier is required");
    }

    [Fact]
    public async Task CreatePickupOrderCommandValidator_ShouldRejectInvalidCharactersInCustomerIdentifier()
    {
        // Arrange
        var validator = new CreatePickupOrderCommandValidator();
        var command = new CreatePickupOrderCommand
        {
            CustomerIdentifier = "customer@123!"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Customer identifier can only contain letters, numbers, hyphens, underscores, @ symbols, and periods");
    }

    [Fact]
    public async Task CreateDeliveryOrderCommandValidator_ShouldAcceptValidRequest()
    {
        // Arrange
        var validator = new CreateDeliveryOrderCommandValidator();
        var command = new CreateDeliveryOrderCommand
        {
            CustomerIdentifier = "customer123",
            AddressLine1 = "123 Test Street",
            Postcode = "SW1A 1AA"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateDeliveryOrderCommandValidator_ShouldRejectInvalidPostcode()
    {
        // Arrange
        var validator = new CreateDeliveryOrderCommandValidator();
        var command = new CreateDeliveryOrderCommand
        {
            CustomerIdentifier = "customer123",
            AddressLine1 = "123 Test Street",
            Postcode = "INVALID"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Postcode must be a valid UK postcode format");
    }

    [Fact]
    public async Task AddItemToOrderCommandValidator_ShouldAcceptValidRequest()
    {
        // Arrange
        var validator = new AddItemToOrderCommandValidator();
        var command = new AddItemToOrderCommand
        {
            OrderIdentifier = "order123",
            RecipeIdentifier = "recipe456",
            Quantity = 2
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddItemToOrderCommandValidator_ShouldRejectInvalidQuantity(int quantity)
    {
        // Arrange
        var validator = new AddItemToOrderCommandValidator();
        var command = new AddItemToOrderCommand
        {
            OrderIdentifier = "order123",
            RecipeIdentifier = "recipe456",
            Quantity = quantity
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Quantity must be greater than 0");
    }

    [Fact]
    public async Task AddItemToOrderCommandValidator_ShouldRejectExcessiveQuantity()
    {
        // Arrange
        var validator = new AddItemToOrderCommandValidator();
        var command = new AddItemToOrderCommand
        {
            OrderIdentifier = "order123",
            RecipeIdentifier = "recipe456",
            Quantity = 15
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Quantity cannot exceed 10 items per single addition");
    }
}