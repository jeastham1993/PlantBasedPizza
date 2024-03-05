using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
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
        public void CanCreateNewDeliveryRequestAddAddDriver_ShouldAddDriverAndRaiseEvent()
        {
            var driverName = "";
            
            DomainEvents.Register<DriverCollectedOrderEvent>((evt) =>
            {
                driverName = evt.DriverName;
            });
            
            var request = new DeliveryRequest(OrderIdentifier, new Address("Address line 1", "TY6 7UI"));

            request.ClaimDelivery("James");

            request.Driver.Should().Be("James");
            request.DriverCollectedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));

            driverName.Should().Be("James");
        }
        
        [Fact]
        public async Task OrderReadyForDeliveryHandler_ShouldStoreNewDeliveryRequest()
        {
            var mockRepo = new Mock<IDeliveryRequestRepository>();
            mockRepo.Setup(p => p.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>()))
                .Verifiable();
            var mockLogger = new Mock<IObservabilityService>();

            var handler = new OrderReadyForDeliveryEventHandler(mockRepo.Object, mockLogger.Object);

            await handler.Handle(new OrderReadyForDeliveryEvent(OrderIdentifier, "Address line 1", string.Empty,
                string.Empty, string.Empty, string.Empty, "TY6 7UI"));
            
            mockRepo.Verify(p => p.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>()), Times.Once);
        }
        
        [Fact]
        public async Task OrderReadyForDeliveryHandlerImmutabilityCheck_ShouldSkipIfOrderAlreadyFound()
        {
            var mockRepo = new Mock<IDeliveryRequestRepository>();
            mockRepo.Setup(p => p.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>()))
                .Verifiable();
            mockRepo.Setup(p => p.GetDeliveryStatusForOrder(It.IsAny<string>()))
                .ReturnsAsync(new DeliveryRequest(OrderIdentifier, new Address("Address line 1", "TY6 7UI")));
            
            var mockLogger = new Mock<IObservabilityService>();

            var handler = new OrderReadyForDeliveryEventHandler(mockRepo.Object, mockLogger.Object);

            await handler.Handle(new OrderReadyForDeliveryEvent(OrderIdentifier, "Address line 1", string.Empty,
                string.Empty, string.Empty, string.Empty, "TY6 7UI"));
            
            mockRepo.Verify(p => p.AddNewDeliveryRequest(It.IsAny<DeliveryRequest>()), Times.Never);
        }
    }
}