using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Deliver.Core.Services;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using Xunit;

namespace PlantBasedPizza.UnitTest
{
    public class DeliveryRequestTests
    {
        internal const string OrderIdentifier = "ORDER123";
        
        [Fact]
        public void CanCreateNewDeliveryRequest_ShouldCreate()
        {
            var request = new DeliveryRequest(OrderIdentifier, new Address("Address line 1", "TY6 7UI"));

            request.OrderIdentifier.Should().Be(OrderIdentifier);
            request.DeliveryAddress.Should().NotBeNull();
            request.DeliveryAddress.AddressLine1.Should().Be("Address line 1");
            request.DeliveryAddress.Postcode.Should().Be("TY6 7UI");
        }
        
        [Fact]
        public async Task CanCreateNewDeliveryRequestAddAddDriver_ShouldAddDriverAndRaiseEvent()
        {
            // Arrange
            var mockEventDispatcher = A.Fake<IDomainEventDispatcher>();
            var deliveryService = new DeliveryDomainService(mockEventDispatcher);
            var request = new DeliveryRequest(OrderIdentifier, new Address("Address line 1", "TY6 7UI"));

            // Act
            await deliveryService.ClaimDeliveryAsync(request, "James");

            // Assert
            request.Driver.Should().Be("James");
            request.DriverCollectedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            A.CallTo(() => mockEventDispatcher.PublishAsync(A<DriverCollectedOrderEvent>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public async Task OrderReadyForDeliveryHandler_ShouldStoreNewDeliveryRequest()
        {
            var mockRepo = A.Fake<IDeliveryRequestRepository>();
            // Set up the repository to return null, indicating no existing delivery request
            A.CallTo(() => mockRepo.GetDeliveryStatusForOrder(A<string>._)).Returns((DeliveryRequest)null);
            var mockLogger = A.Fake<ILogger<OrderReadyForDeliveryEventHandler>>();

            var handler = new OrderReadyForDeliveryEventHandler(mockRepo, mockLogger);

            await handler.Handle(new OrderReadyForDeliveryEvent(OrderIdentifier, "Address line 1", string.Empty,
                string.Empty, string.Empty, string.Empty, "TY6 7UI"));
            
            A.CallTo(() => mockRepo.AddNewDeliveryRequest(A<DeliveryRequest>._)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public async Task OrderReadyForDeliveryHandlerImmutabilityCheck_ShouldSkipIfOrderAlreadyFound()
        {
            var mockRepo = A.Fake<IDeliveryRequestRepository>();
            A.CallTo(() => mockRepo.GetDeliveryStatusForOrder(A<string>._))
                .Returns(new DeliveryRequest(OrderIdentifier, new Address("Address line 1", "TY6 7UI")));
            
            var mockLogger = A.Fake<ILogger<OrderReadyForDeliveryEventHandler>>();

            var handler = new OrderReadyForDeliveryEventHandler(mockRepo, mockLogger);

            await handler.Handle(new OrderReadyForDeliveryEvent(OrderIdentifier, "Address line 1", string.Empty,
                string.Empty, string.Empty, string.Empty, "TY6 7UI"));
            
            A.CallTo(() => mockRepo.AddNewDeliveryRequest(A<DeliveryRequest>._)).MustNotHaveHappened();
        }
    }
}