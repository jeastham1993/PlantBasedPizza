using Microsoft.AspNetCore.Authorization;
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
        private readonly IKitchenEventPublisher _eventPublisher;
        private readonly IObservabilityService _observabilityService;

        public KitchenController(IKitchenRequestRepository kitchenRequestRepository, IObservabilityService observabilityService, IKitchenEventPublisher eventPublisher)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
            this._observabilityService = observabilityService;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Get a list of all new kitchen requests.
        /// </summary>
        /// <returns></returns>
        [HttpGet("new")]
        [Authorize(Roles = "staff")]
        public IEnumerable<KitchenRequestDto> GetNew()
        {
            try
            {
                var queryResults = this._kitchenRequestRepository.GetNew().Result;

                return queryResults.Select(p => new KitchenRequestDto(p)).ToList();
            }
            catch (Exception ex)
            {
                this._observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDto>();
            }
        }

        /// <summary>
        /// Mark an order has being prepared.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/preparing")]
        [Authorize(Roles = "staff")]
        public async Task<KitchenRequest> Preparing(string orderIdentifier)
        {
            ApplicationLogger.Info("Received request to prepare order");

            var kitchenRequest = this._kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.Preparing(this.Request.Headers["CorrelationId"].ToString());

            await this._kitchenRequestRepository.Update(kitchenRequest);
            await this._eventPublisher.PublishOrderPreparingEventV1(kitchenRequest);

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders that are currently being prepared.
        /// </summary>
        /// <returns></returns>
        [HttpGet("prep")]
        [Authorize(Roles = "staff")]
        public IEnumerable<KitchenRequestDto> GetPrep()
        {
            try
            {
                var queryResults = this._kitchenRequestRepository.GetPrep().Result;

                return queryResults.Select(p => new KitchenRequestDto(p)).ToList();
            }
            catch (Exception ex)
            {
                this._observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDto>();
            }
        }

        /// <summary>
        /// Mark an order as being prepared.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/prep-complete")]
        [Authorize(Roles = "staff")]
        public async Task<KitchenRequest> PrepComplete(string orderIdentifier)
        {
            var kitchenRequest = this._kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.PrepComplete(this.Request.Headers["CorrelationId"].ToString());

            await this._kitchenRequestRepository.Update(kitchenRequest);
            await this._eventPublisher.PublishOrderPrepCompleteEventV1(kitchenRequest);

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders currently baking.
        /// </summary>
        /// <returns></returns>
        [HttpGet("baking")]
        [Authorize(Roles = "staff")]
        public IEnumerable<KitchenRequestDto> GetBaking()
        {
            try
            {
                var queryResults = this._kitchenRequestRepository.GetBaking().Result;

                return queryResults.Select(p => new KitchenRequestDto(p));
            }
            catch (Exception ex)
            {
                this._observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDto>();
            }
        }

        /// <summary>
        /// Mark an order as bake complete.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/bake-complete")]
        [Authorize(Roles = "staff")]
        public async Task<KitchenRequest> BakeComplete(string orderIdentifier)
        {
            var kitchenRequest = this._kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.BakeComplete(this.Request.Headers["CorrelationId"].ToString());

            await this._kitchenRequestRepository.Update(kitchenRequest);
            await this._eventPublisher.PublishOrderBakedEventV1(kitchenRequest);

            return kitchenRequest;
        }

        /// <summary>
        /// Mark an order as quality check completed.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("{orderIdentifier}/quality-check")]
        [Authorize(Roles = "staff")]
        public async Task<KitchenRequest> QualityCheckComplete(string orderIdentifier)
        {
            var kitchenRequest = this._kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.QualityCheckComplete(this.Request.Headers["CorrelationId"].ToString()).Wait();

            await this._kitchenRequestRepository.Update(kitchenRequest);
            await this._eventPublisher.PublishOrderQualityCheckedEventV1(kitchenRequest);

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders awaiting quality check.
        /// </summary>
        /// <returns></returns>
        [HttpGet("quality-check")]
        [Authorize(Roles = "staff")]
        public IEnumerable<KitchenRequestDto> GetAwaitingQualityCheck()
        {
            try
            {
                var queryResults = this._kitchenRequestRepository.GetAwaitingQualityCheck().Result;

                return queryResults.Select(p => new KitchenRequestDto(p));
            }
            catch (Exception ex)
            {
                this._observabilityService.Error(ex, "Error processing");
                return new List<KitchenRequestDto>();
            }
        }
    }
}