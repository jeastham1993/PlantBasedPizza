using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core.ConfirmOrder
{
    public class ConfirmOrderCommand
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
        
        [JsonPropertyName("paymentAmount")]
        public decimal PaymentAmount { get; init; }
    }
}