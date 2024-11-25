using System.Text.Json.Serialization;

namespace PlantBasedPizza.Deliver.Core.AssignDriver
{
    public class AssignDriverRequest
    {
        [JsonPropertyName("OrderIdentifier")]
        public string OrderIdentifier { get; init; } = "";
        
        [JsonPropertyName("DriverName")]
        public string DriverName { get; init; } = "";
    }
}