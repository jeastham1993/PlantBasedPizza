using System.Text.Json.Serialization;

namespace PlantBasedPizza.Deliver.Core.Commands
{
    public class MarkOrderDeliveredRequest
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
    }
}