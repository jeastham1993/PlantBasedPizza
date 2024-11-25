using System.Text.Json.Serialization;

namespace PlantBasedPizza.Deliver.Core.MarkOrderDelivered
{
    public class MarkOrderDeliveredRequest
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
    }
}