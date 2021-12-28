using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Infrastructure.Controllers
{
    public class KitchenController : ControllerBase
    {
        private readonly IKitchenRequestRepository _kitchenRequestRepository;

        public KitchenController(IKitchenRequestRepository kitchenRequestRepository)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
        }

        /// <summary>
        /// Get a list of all new kitchen requests.
        /// </summary>
        /// <returns></returns>
        [HttpGet("kitchen/new")]
        public async Task<IEnumerable<KitchenRequest>> GetNew()
        {
            return await this._kitchenRequestRepository.GetNew().ConfigureAwait(false);
        }

        /// <summary>
        /// Mark an order has being prepared.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("kitchen/{orderIdentifier}/preparing")]
        public async Task<KitchenRequest> Preparing(string orderIdentifier)
        {
            var kitchenRequest = await this._kitchenRequestRepository.Retrieve(orderIdentifier);

            kitchenRequest.Preparing(this.Request.Headers["CorrelationId"].ToString());

            await this._kitchenRequestRepository.Update(kitchenRequest);

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders that are currently being prepared.
        /// </summary>
        /// <returns></returns>
        [HttpGet("kitchen/prep")]
        public async Task<IEnumerable<KitchenRequest>> GetPrep()
        {
            return await this._kitchenRequestRepository.GetPrep().ConfigureAwait(false);
        }

        /// <summary>
        /// Mark an order as being prepared.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("kitchen/{orderIdentifier}/prep-complete")]
        public async Task<KitchenRequest> PrepComplete(string orderIdentifier)
        {
            var kitchenRequest = await this._kitchenRequestRepository.Retrieve(orderIdentifier);

            kitchenRequest.PrepComplete(this.Request.Headers["CorrelationId"].ToString());

            await this._kitchenRequestRepository.Update(kitchenRequest);

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders currently baking.
        /// </summary>
        /// <returns></returns>
        [HttpGet("kitchen/baking")]
        public async Task<IEnumerable<KitchenRequest>> GetBaking()
        {
            return await this._kitchenRequestRepository.GetBaking().ConfigureAwait(false);
        }

        /// <summary>
        /// Mark an order as bake complete.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("kitchen/{orderIdentifier}/bake-complete")]
        public async Task<KitchenRequest> BakeComplete(string orderIdentifier)
        {
            var kitchenRequest = await this._kitchenRequestRepository.Retrieve(orderIdentifier);

            kitchenRequest.BakeComplete(this.Request.Headers["CorrelationId"].ToString());

            await this._kitchenRequestRepository.Update(kitchenRequest);

            return kitchenRequest;
        }

        /// <summary>
        /// Mark an order as quality check completed.
        /// </summary>
        /// <param name="orderIdentifier">The order identifier.</param>
        /// <returns></returns>
        [HttpPut("kitchen/{orderIdentifier}/quality-check")]
        public async Task<KitchenRequest> QualityCheckComplete(string orderIdentifier)
        {
            var kitchenRequest = await this._kitchenRequestRepository.Retrieve(orderIdentifier);

            await kitchenRequest.QualityCheckComplete(this.Request.Headers["CorrelationId"].ToString());

            await this._kitchenRequestRepository.Update(kitchenRequest);

            return kitchenRequest;
        }

        /// <summary>
        /// List all orders awaiting quality check.
        /// </summary>
        /// <returns></returns>
        [HttpGet("kitchen/quality-check")]
        public async Task<IEnumerable<KitchenRequest>> GetAwaitingQualityCheck()
        {
            return await this._kitchenRequestRepository.GetAwaitingQualityCheck().ConfigureAwait(false);
        }
    }
}