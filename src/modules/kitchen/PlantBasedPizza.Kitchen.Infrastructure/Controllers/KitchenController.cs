using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Kitchen.Infrastructure.Controllers
{
    [Route("kitchen")]
    public class KitchenController : ControllerBase
    {
        private readonly IKitchenRequestRepository _kitchenRequestRepository;
        private readonly IObservabilityService _observabilityService;

        public KitchenController(IKitchenRequestRepository kitchenRequestRepository, IObservabilityService observabilityService)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
            this._observabilityService = observabilityService;
        }

        /// <summary>
        /// Get a list of all new kitchen requests.
        /// </summary>
        /// <returns></returns>
        [HttpGet("new")]
        public IEnumerable<KitchenRequestDTO> GetNew()
        {
            try
            {
                var queryResults = this._kitchenRequestRepository.GetNew().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
            }
            catch (Exception ex)
            {
                this._observabilityService.Error(ex, "Error processing");
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

            var kitchenRequest = this._kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.Preparing(this.Request.Headers["CorrelationId"].ToString());

            this._kitchenRequestRepository.Update(kitchenRequest).Wait();

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
                var queryResults = this._kitchenRequestRepository.GetPrep().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p)).ToList();
            }
            catch (Exception ex)
            {
                this._observabilityService.Error(ex, "Error processing");
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
            var kitchenRequest = this._kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.PrepComplete(this.Request.Headers["CorrelationId"].ToString());

            this._kitchenRequestRepository.Update(kitchenRequest).Wait();

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
                var queryResults = this._kitchenRequestRepository.GetBaking().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p));
            }
            catch (Exception ex)
            {
                this._observabilityService.Error(ex, "Error processing");
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
            var kitchenRequest = this._kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.BakeComplete(this.Request.Headers["CorrelationId"].ToString());

            this._kitchenRequestRepository.Update(kitchenRequest).Wait();

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
            var kitchenRequest = this._kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.QualityCheckComplete(this.Request.Headers["CorrelationId"].ToString()).Wait();

            this._kitchenRequestRepository.Update(kitchenRequest).Wait();

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
                var queryResults = this._kitchenRequestRepository.GetAwaitingQualityCheck().Result;

                return queryResults.Select(p => new KitchenRequestDTO(p));
            }
            catch (Exception ex)
            {
                this._observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDTO>();
            }
        }
    }
}