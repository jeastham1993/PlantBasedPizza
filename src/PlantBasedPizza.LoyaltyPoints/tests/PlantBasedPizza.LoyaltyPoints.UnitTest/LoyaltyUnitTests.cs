using FakeItEasy;
using FluentAssertions;
using PlantBasedPizza.LoyaltyPoints.Core;

namespace PlantBasedPizza.LoyaltyPoints.UnitTest;

public class LoyaltyUnitTests
{
    [Fact]
    public async Task CanAddLoyaltyPoints_ShouldReturnValidObject()
    {
        var mockRepo = A.Fake<ICustomerLoyaltyPointsRepository>();
        var customerId = "james";
        CustomerLoyaltyPoints? response = null;
        
        A.CallTo(() => mockRepo.GetCurrentPointsFor(customerId)).Returns(response);
        
        var handler = new AddLoyaltyPointsCommandHandler(mockRepo);

        var handleResponse = await handler.Handle(new AddLoyaltyPointsCommand()
        {
            CustomerIdentifier = customerId,
            OrderValue = 50.79M
        });

        handleResponse.CustomerIdentifier.Should().Be(customerId);
        handleResponse.TotalPoints.Should().Be(51);
    }
    
    [Fact]
    public async Task CanAddLoyaltyPointsForExisting_ShouldReturnValidObject()
    {
        var mockRepo = A.Fake<ICustomerLoyaltyPointsRepository>();
        var customerId = "james";
        CustomerLoyaltyPoints response = new CustomerLoyaltyPoints()
        {
            TotalPoints = 150,
            CustomerId = "james"
        };
        
        A.CallTo(() => mockRepo.GetCurrentPointsFor(customerId)).Returns(response);
        
        var handler = new AddLoyaltyPointsCommandHandler(mockRepo);

        var handleResponse = await handler.Handle(new AddLoyaltyPointsCommand()
        {
            CustomerIdentifier = customerId,
            OrderValue = 50.79M
        });

        handleResponse.CustomerIdentifier.Should().Be(customerId);
        handleResponse.TotalPoints.Should().Be(201);
    }
    
    [Fact]
    public async Task CanSpendPoints_ShouldDecreaseFromBalance()
    {
        var mockRepo = A.Fake<ICustomerLoyaltyPointsRepository>();
        var customerId = "james";
        CustomerLoyaltyPoints response = new CustomerLoyaltyPoints()
        {
            TotalPoints = 150,
            CustomerId = "james"
        };
        
        A.CallTo(() => mockRepo.GetCurrentPointsFor(customerId)).Returns(response);
        
        var handler = new SpendLoyaltyPointsCommandHandler(mockRepo);

        var handleResponse = await handler.Handle(new SpendLoyaltyPointsCommand()
        {
            CustomerIdentifier = customerId,
            PointsToSpend = 50
        });

        handleResponse.CustomerIdentifier.Should().Be(customerId);
        handleResponse.TotalPoints.Should().Be(100);
    }
    
    [Fact]
    public async Task CanSpendPointsThatAreOver_ShouldError()
    {
        var mockRepo = A.Fake<ICustomerLoyaltyPointsRepository>();
        var customerId = "james";
        CustomerLoyaltyPoints response = new CustomerLoyaltyPoints()
        {
            TotalPoints = 10,
            CustomerId = "james"
        };
        
        A.CallTo(() => mockRepo.GetCurrentPointsFor(customerId)).Returns(response);
        
        var handler = new SpendLoyaltyPointsCommandHandler(mockRepo);
        
        var act = async () => await handler.Handle(new SpendLoyaltyPointsCommand()
        {
            CustomerIdentifier = customerId,
            PointsToSpend = 50
        });

        await act.Should().ThrowAsync<InsufficientPointsException>();
    }
}