using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.Kitchen.Core.Entities
{
    public class KitchenRequest
    {
        [JsonConstructor]
        private KitchenRequest()
        {
        }
        
        public KitchenRequest(string orderIdentifier, List<RecipeAdapter> recipes)
        {
            Guard.AgainstNullOrEmpty(orderIdentifier, nameof(orderIdentifier));
            
            this.KitchenRequestId = Guid.NewGuid().ToString();
            this.OrderIdentifier = orderIdentifier;
            this.OrderReceivedOn = DateTime.Now;
            this.OrderState = OrderState.NEW;
            this.Recipes = recipes;
        }
        
        [JsonProperty]
        public string KitchenRequestId { get; private set; }
        
        [JsonProperty]
        public string OrderIdentifier { get; private set; }
        
        [JsonProperty]
        public DateTime OrderReceivedOn { get; private set; }
        
        [JsonProperty]
        public OrderState OrderState { get; private set; }
        
        [JsonProperty]
        public DateTime? PrepCompleteOn { get; private set; }
        
        [JsonProperty]
        public DateTime? BakeCompleteOn { get; private set; }
        
        [JsonProperty]
        public DateTime? QualithCheckCompleteOn { get; private set; }
        
        [JsonProperty]
        public List<RecipeAdapter> Recipes { get; private set; }

        public void Preparing(string correlationId = "")
        {
            this.OrderState = OrderState.PREPARING;

            DomainEvents.Raise(new OrderPreparingEvent(this.OrderIdentifier)
            {
                CorrelationId = correlationId
            });
        }

        public void PrepComplete(string correlationId = "")
        {
            this.OrderState = OrderState.BAKING;
            
            this.PrepCompleteOn = DateTime.Now;
            
            DomainEvents.Raise(new OrderPrepCompleteEvent(this.OrderIdentifier)
            {
                CorrelationId = correlationId
            });
        }

        public void BakeComplete(string correlationId = "")
        {
            this.OrderState = OrderState.QUALITYCHECK;
            
            this.BakeCompleteOn = DateTime.Now;
            
            DomainEvents.Raise(new OrderBakedEvent(this.OrderIdentifier)
            {
                CorrelationId = correlationId
            });
        }

        public async Task QualityCheckComplete(string correlationId = "")
        {
            this.OrderState = OrderState.DONE;
            
            this.QualithCheckCompleteOn = DateTime.Now;

            await DomainEvents.Raise(new OrderQualityCheckedEvent(this.OrderIdentifier)
            {
                CorrelationId = correlationId
            });
        }
    }
}