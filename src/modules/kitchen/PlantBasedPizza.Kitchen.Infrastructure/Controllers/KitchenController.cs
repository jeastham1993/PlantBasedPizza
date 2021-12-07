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

        [HttpGet("kitchen/new")]
        public async Task<IEnumerable<KitchenRequest>> GetNew()
        {
            return await this._kitchenRequestRepository.GetNew().ConfigureAwait(false);
        }

        [HttpPut("kitchen/{orderIdentifier}/preparing")]
        public async Task<KitchenRequest> Preparing(string orderIdentifier)
        {
            var kitchenRequest = await this._kitchenRequestRepository.Retrieve(orderIdentifier);

            kitchenRequest.Preparing();

            await this._kitchenRequestRepository.Update(kitchenRequest);

            return kitchenRequest;
        }

        [HttpGet("kitchen/prep")]
        public async Task<IEnumerable<KitchenRequest>> GetPrep()
        {
            return await this._kitchenRequestRepository.GetPrep().ConfigureAwait(false);
        }

        [HttpPut("kitchen/{orderIdentifier}/prep-complete")]
        public async Task<KitchenRequest> PrepComplete(string orderIdentifier)
        {
            var kitchenRequest = await this._kitchenRequestRepository.Retrieve(orderIdentifier);

            kitchenRequest.PrepComplete();

            await this._kitchenRequestRepository.Update(kitchenRequest);

            return kitchenRequest;
        }

        [HttpGet("kitchen/baking")]
        public async Task<IEnumerable<KitchenRequest>> GetBaking()
        {
            return await this._kitchenRequestRepository.GetBaking().ConfigureAwait(false);
        }

        [HttpPut("kitchen/{orderIdentifier}/bake-complete")]
        public async Task<KitchenRequest> BakeComplete(string orderIdentifier)
        {
            var kitchenRequest = await this._kitchenRequestRepository.Retrieve(orderIdentifier);

            kitchenRequest.BakeComplete();

            await this._kitchenRequestRepository.Update(kitchenRequest);

            return kitchenRequest;
        }

        [HttpPut("kitchen/{orderIdentifier}/quality-check")]
        public async Task<KitchenRequest> QualityCheckComplete(string orderIdentifier)
        {
            var kitchenRequest = await this._kitchenRequestRepository.Retrieve(orderIdentifier);

            await kitchenRequest.QualityCheckComplete();

            await this._kitchenRequestRepository.Update(kitchenRequest);

            return kitchenRequest;
        }

        [HttpGet("kitchen/quality-check")]
        public async Task<IEnumerable<KitchenRequest>> GetAwaitingQualityCheck()
        {
            return await this._kitchenRequestRepository.GetAwaitingQualityCheck().ConfigureAwait(false);
        }
    }
}