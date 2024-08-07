using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Deliver.Core.Commands;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.GetDelivery;
using PlantBasedPizza.Deliver.Core.IntegrationEvents;
using PlantBasedPizza.Deliver.Infrastructure.IntegrationEvents;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Deliver.Infrastructure.Controllers
{
    [Route("delivery")]
    [EnableCors("CorsPolicy")]
    public class DeliveryRequestController : ControllerBase 
    {
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;
        private readonly IDeliveryEventPublisher _eventPublisher;
        private readonly GetDeliveryQueryHandler _getDeliveryQueryHandler;

        public DeliveryRequestController(IDeliveryRequestRepository deliveryRequestRepository, GetDeliveryQueryHandler getDeliveryQueryHandler, IDeliveryEventPublisher eventPublisher)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _getDeliveryQueryHandler = getDeliveryQueryHandler;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Get the status of a specific order.
        /// </summary>
        /// <param name="orderIdentifier">The identifier of the order.</param>
        /// <returns>A <see cref="DeliveryRequest"/>.</returns>
        [HttpGet("{orderIdentifier}/status")]
        [Authorize(Roles = "user")]
        public async Task<DeliveryRequestDto?> Get(string orderIdentifier)
        {
            return await this._getDeliveryQueryHandler.Handle(new GetDeliveryQuery(orderIdentifier));
        }

        /// <summary>
        /// Get all of the orders currently awaiting collection by a driver.
        /// </summary>
        /// <returns>A list of all orders awaiting collection.</returns>
        [HttpGet("awaiting-collection")]
        [Authorize(Roles = "staff")]
        public async Task<List<DeliveryRequest>> GetAwaitingCollection()
        {
            return await this._deliveryRequestRepository.GetAwaitingDriver();
        }

        /// <summary>
        /// Assigne a driver to a given order.
        /// </summary>
        /// <param name="request">The contents of the assignment request. A <see cref="AssignDriverRequest"/>.</param>
        /// <returns>The status.</returns>
        [HttpPost("assign")]
        [Authorize(Roles = "staff")]
        public async Task<IActionResult> Collect([FromBody] AssignDriverRequest request)
        {
            request.AddToTelemetry();
            
            var existingDeliveryRequest = await this._deliveryRequestRepository.GetDeliveryStatusForOrder(request.OrderIdentifier);

            if (existingDeliveryRequest == null)
            {
                return this.NotFound();
            }

            await existingDeliveryRequest.ClaimDelivery(request.DriverName, this.Request.Headers["CorrelationId"].ToString());

            await this._deliveryRequestRepository.UpdateDeliveryRequest(existingDeliveryRequest);
            await this._eventPublisher.PublishDriverOrderCollectedEventV1(existingDeliveryRequest);

            return this.Ok(existingDeliveryRequest);
        }

        /// <summary>
        /// Mark an order as being delivered.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("delivered")]
        [Authorize(Roles = "driver")]
        public async Task<IActionResult> MarkDelivered([FromBody] MarkOrderDeliveredRequest request)
        {
            request.AddToTelemetry();
            
            var existingDeliveryRequest = await this._deliveryRequestRepository.GetDeliveryStatusForOrder(request.OrderIdentifier);

            if (existingDeliveryRequest == null)
            {
                return this.NotFound();
            }

            await existingDeliveryRequest.Deliver(this.Request.Headers["CorrelationId"].ToString());
            
            await this._deliveryRequestRepository.UpdateDeliveryRequest(existingDeliveryRequest);
            await this._eventPublisher.PublishDriverDeliveredOrderEventV1(existingDeliveryRequest);

            return this.Ok(existingDeliveryRequest);
        }

        /// <summary>
        /// Get all of the orders currently with a specific driver.
        /// </summary>
        /// <param name="driverName">The name of the driver to search for.</param>
        /// <returns></returns>
        [HttpGet("driver/{driverName}/orders")]
        [Authorize(Roles = "staff")]
        public async Task<List<DeliveryRequest>> GetForDriver(string driverName)
        {
            Activity.Current?.AddTag("driverName", driverName);
            
            return await this._deliveryRequestRepository.GetOrdersWithDriver(driverName);
        }
    }
}