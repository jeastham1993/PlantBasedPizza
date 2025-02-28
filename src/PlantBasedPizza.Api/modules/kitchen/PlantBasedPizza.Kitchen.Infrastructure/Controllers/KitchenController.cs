using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Kitchen.Infrastructure.Controllers
{
    [Route("kitchen")]
    public class KitchenController(
        IKitchenRequestRepository kitchenRequestRepository,
        IObservabilityService observabilityService)
        : ControllerBase
    {
        /// <summary>
        /// Get a list of all new kitchen requests.
        /// </summary>
        /// <returns></returns>
        [HttpGet("new")]
        public IEnumerable<KitchenRequestDTO> GetNew()
        {
            try
            {
                var queryResults = kitchenRequestRepository.GetNew().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
            }
            catch (Exception ex)
            {
                observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }

        /// <summary>
        /// Mark an order has being prepared.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/preparing")]
        public KitchenRequest Preparing(string orderIdentifier)
        {
            ApplicationLogger.Info("Received request to prepare order");

            var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.Preparing(this.Request.Headers["CorrelationId"].ToString());

            kitchenRequestRepository.Update(kitchenRequest).Wait();

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders that are currently being prepared.
        /// </summary>
        /// <returns></returns>
        [HttpGet("prep")]
        public IEnumerable<KitchenRequestDTO> GetPrep()
        {
            try
            {
                var queryResults = kitchenRequestRepository.GetPrep().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
            }
            catch (Exception ex)
            {
                observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }

        /// <summary>
        /// Mark an order as being prepared.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/prep-complete")]
        public KitchenRequest PrepComplete(string orderIdentifier)
        {
            var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.PrepComplete(this.Request.Headers["CorrelationId"].ToString());

            kitchenRequestRepository.Update(kitchenRequest).Wait();

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders currently baking.
        /// </summary>
        /// <returns></returns>
        [HttpGet("baking")]
        public IEnumerable<KitchenRequestDTO> GetBaking()
        {
            try
            {
                var queryResults = kitchenRequestRepository.GetBaking().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p));
            }
            catch (Exception ex)
            {
                observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }

        /// <summary>
        /// Mark an order as bake complete.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/bake-complete")]
        public KitchenRequest BakeComplete(string orderIdentifier)
        {
            var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.BakeComplete(this.Request.Headers["CorrelationId"].ToString());

            kitchenRequestRepository.Update(kitchenRequest).Wait();

            return kitchenRequest;
        }

        /// <summary>
        /// Mark an order as quality check completed.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/quality-check")]
        public KitchenRequest QualityCheckComplete(string orderIdentifier)
        {
            var kitchenRequest = kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.QualityCheckComplete(this.Request.Headers["CorrelationId"].ToString()).Wait();

            kitchenRequestRepository.Update(kitchenRequest).Wait();

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders awaiting quality check.
        /// </summary>
        /// <returns></returns>
        [HttpGet("quality-check")]
        public IEnumerable<KitchenRequestDTO> GetAwaitingQualityCheck()
        {
            try
            {
                var queryResults = kitchenRequestRepository.GetAwaitingQualityCheck().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p));
            }
            catch (Exception ex)
            {
                observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }
    }
}