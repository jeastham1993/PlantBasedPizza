using System.Text.Json.Serialization;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.Kitchen.Core.Entities
{
    public class KitchenRequest
    {
        [JsonConstructor]
        private KitchenRequest()
        {
            this.Recipes = new List<RecipeAdapter>();
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
        
        [JsonPropertyName("kitchenRequestId")]
        public string KitchenRequestId { get; private set; } = "";
        
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; private set; } = "";
        
        [JsonPropertyName("orderReceivedOn")]
        public DateTime OrderReceivedOn { get; private set; }
        
        [JsonPropertyName("orderState")]
        public OrderState OrderState { get; private set; }
        
        [JsonPropertyName("prepCompleteOn")]
        public DateTime? PrepCompleteOn { get; private set; }
        
        [JsonPropertyName("bakeCompleteOn")]
        public DateTime? BakeCompleteOn { get; private set; }
        
        [JsonPropertyName("qualityCheckCompleteOn")]
        public DateTime? QualityCheckCompleteOn { get; private set; }
        
        [JsonPropertyName("recipes")]
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
            
            this.QualityCheckCompleteOn = DateTime.Now;
        }
    }
}