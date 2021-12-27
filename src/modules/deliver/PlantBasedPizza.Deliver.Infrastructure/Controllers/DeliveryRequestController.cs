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

        [HttpGet("delivery/{orderIdentifier}/status")]
        public async Task<DeliveryRequest?> Get(string orderIdentifier)
        {
            return await this._deliveryRequestRepository.GetDeliveryStatusForOrder(orderIdentifier);
        }

        [HttpGet("delivery/awaiting-collection")]
        public async Task<List<DeliveryRequest>> GetAwaitingCollection()
        {
            this._logger.Info("Received request to get orders awaiting collection");
            
            return await this._deliveryRequestRepository.GetAwaitingDriver();
        }

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

        [HttpGet("delivery/driver/{driverName}/orders")]
        public async Task<List<DeliveryRequest>> GetForDriver(string driverName)
        {
            return await this._deliveryRequestRepository.GetOrdersWithDriver(driverName);
        }
    }
}