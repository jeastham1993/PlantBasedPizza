using System.Text.Json.Serialization;

namespace PlantBasedPizza.Deliver.Core.Commands
{
    public class AssignDriverRequest
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
        
        [JsonPropertyName("DriverName")]
        public string DriverName { get; init; } = "";
    }
}