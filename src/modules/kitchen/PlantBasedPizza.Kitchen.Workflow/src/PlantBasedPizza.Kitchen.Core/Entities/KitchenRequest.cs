using Newtonsoft.Json;
using PlantBasedPizza.Kitchen.Core.Adapters;

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
            this.KitchenRequestId = Guid.NewGuid().ToString();
            this.OrderIdentifier = orderIdentifier ?? throw new ArgumentNullException(nameof(orderIdentifier));
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
        }

        public void PrepComplete(string correlationId = "")
        {
            this.OrderState = OrderState.BAKING;
            
            this.PrepCompleteOn = DateTime.Now;
        }

        public void BakeComplete(string correlationId = "")
        {
            this.OrderState = OrderState.QUALITYCHECK;
            
            this.BakeCompleteOn = DateTime.Now;
        }

        public async Task QualityCheckComplete(string correlationId = "")
        {
            this.OrderState = OrderState.DONE;
            
            this.QualithCheckCompleteOn = DateTime.Now;
        }
    }
}