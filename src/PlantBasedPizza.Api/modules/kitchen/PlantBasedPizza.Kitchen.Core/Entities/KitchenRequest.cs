using System.Text.Json.Serialization;
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
            this.Recipes = new List<RecipeAdapter>();
        }
        
        public KitchenRequest(string orderIdentifier, List<RecipeAdapter> recipes)
        {
            Guard.AgainstNullOrEmpty(orderIdentifier, nameof(orderIdentifier));
            
            this.KitchenRequestId = Guid.NewGuid().ToString();
            this.OrderIdentifier = orderIdentifier;
            this.OrderReceivedOn = DateTime.Now.ToUniversalTime();
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

        public void StartPreparing()
        {
            this.OrderState = OrderState.PREPARING;
        }

        public void CompletePreparing()
        {
            this.OrderState = OrderState.BAKING;
            this.PrepCompleteOn = DateTime.UtcNow;
        }

        public void CompleteBaking()
        {
            this.OrderState = OrderState.QUALITYCHECK;
            this.BakeCompleteOn = DateTime.UtcNow;
        }

        public void CompleteQualityCheck()
        {
            this.OrderState = OrderState.DONE;
            this.QualityCheckCompleteOn = DateTime.UtcNow;
        }
    }
}