using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;

namespace PlantBasedPizza.Kitchen.Infrastructure.Controllers
{
    [Route("kitchen")]
    public class KitchenController : ControllerBase
    {
        private readonly IKitchenRequestRepository _kitchenRequestRepository;
        private readonly IKitchenEventPublisher _eventPublisher;

        public KitchenController(IKitchenRequestRepository kitchenRequestRepository, IKitchenEventPublisher eventPublisher)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
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
                var queryResults = _kitchenRequestRepository.GetNew().Result;

                return queryResults.Select(p => new KitchenRequestDto(p)).ToList();
            }
            catch (Exception ex)
            {
                Activity.Current?.AddException(ex);
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
            var kitchenRequest = _kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.Preparing(Request.Headers["CorrelationId"].ToString());

            await _kitchenRequestRepository.Update(kitchenRequest);
            await _eventPublisher.PublishOrderPreparingEventV1(kitchenRequest);

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
                var queryResults = _kitchenRequestRepository.GetPrep().Result;

                return queryResults.Select(p => new KitchenRequestDto(p)).ToList();
            }
            catch (Exception ex)
            {
                Activity.Current?.AddException(ex);
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
            var kitchenRequest = _kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.PrepComplete(Request.Headers["CorrelationId"].ToString());

            await _kitchenRequestRepository.Update(kitchenRequest);
            await _eventPublisher.PublishOrderPrepCompleteEventV1(kitchenRequest);

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
                var queryResults = _kitchenRequestRepository.GetBaking().Result;

                return queryResults.Select(p => new KitchenRequestDto(p));
            }
            catch (Exception ex)
            {
                Activity.Current?.AddException(ex);
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
            var kitchenRequest = _kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.BakeComplete(Request.Headers["CorrelationId"].ToString());

            await _kitchenRequestRepository.Update(kitchenRequest);
            await _eventPublisher.PublishOrderBakedEventV1(kitchenRequest);

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
            var kitchenRequest = _kitchenRequestRepository.Retrieve(orderIdentifier).Result;

            kitchenRequest.QualityCheckComplete(Request.Headers["CorrelationId"].ToString()).Wait();

            await _kitchenRequestRepository.Update(kitchenRequest);
            await _eventPublisher.PublishOrderQualityCheckedEventV1(kitchenRequest);

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
                var queryResults = _kitchenRequestRepository.GetAwaitingQualityCheck().Result;

                return queryResults.Select(p => new KitchenRequestDto(p));
            }
            catch (Exception ex)
            {
                Activity.Current?.AddException(ex);
                return new List<KitchenRequestDto>();
            }
        }
    }
}