using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Deliver.Core.Commands;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Deliver.Infrastructure.Controllers
{
    [Route("delivery")]
    public class DeliveryRequestController : ControllerBase 
    {
        private readonly IDeliveryRequestRepository _deliveryRequestRepository;
        private readonly IObservabilityService _logger;

        public DeliveryRequestController(IDeliveryRequestRepository deliveryRequestRepository, IObservabilityService logger)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get the status of a specific order.
        /// </summary>
        /// <param name="orderIdentifier">The identifier of the order.</param>
        /// <returns>A <see cref="DeliveryRequest"/>.</returns>
        [HttpGet("delivery/{orderIdentifier}/status")]
        public async Task<DeliveryRequest?> Get(string orderIdentifier)
        {
            return await this._deliveryRequestRepository.GetDeliveryStatusForOrder(orderIdentifier);
        }

        /// <summary>
        /// Get all of the orders currently awaiting collection by a driver.
        /// </summary>
        /// <returns>A list of all orders awaiting collection.</returns>
        [HttpGet("delivery/awaiting-collection")]
        public async Task<List<DeliveryRequest>> GetAwaitingCollection()
        {
            this._logger.Info("Received request to get orders awaiting collection");
            
            return await this._deliveryRequestRepository.GetAwaitingDriver();
        }

        /// <summary>
        /// Assigne a driver to a given order.
        /// </summary>
        /// <param name="request">The contents of the assignment request. A <see cref="AssignDriverRequest"/>.</param>
        /// <returns>The status.</returns>
        [HttpPost("delivery/assign")]
        public async Task<IActionResult> Collect([FromBody] AssignDriverRequest request)
        {
            var existingDeliveryRequest = await this._deliveryRequestRepository.GetDeliveryStatusForOrder(request.OrderIdentifier);

            if (existingDeliveryRequest == null)
            {
                return this.NotFound();
            }

            await existingDeliveryRequest.ClaimDelivery(request.DriverName, this.Request.Headers["CorrelationId"].ToString());

            await this._deliveryRequestRepository.UpdateDeliveryRequest(existingDeliveryRequest);

            return this.Ok(existingDeliveryRequest);
        }

        /// <summary>
        /// Mark an order as being delivered.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("delivery/delivered")]
        public async Task<IActionResult> MarkDelivered([FromBody] MarkOrderDeliveredRequest request)
        {
            var existingDeliveryRequest = await this._deliveryRequestRepository.GetDeliveryStatusForOrder(request.OrderIdentifier);

            if (existingDeliveryRequest == null)
            {
                return this.NotFound();
            }

            await existingDeliveryRequest.Deliver(this.Request.Headers["CorrelationId"].ToString());
            
            await this._deliveryRequestRepository.UpdateDeliveryRequest(existingDeliveryRequest);

            return this.Ok(existingDeliveryRequest);
        }

        /// <summary>
        /// Get all of the orders currently with a specific driver.
        /// </summary>
        /// <param name="driverName">The name of the driver to search for.</param>
        /// <returns></returns>
        [HttpGet("delivery/driver/{driverName}/orders")]
        public async Task<List<DeliveryRequest>> GetForDriver(string driverName)
        {
            return await this._deliveryRequestRepository.GetOrdersWithDriver(driverName);
        }
    }
}