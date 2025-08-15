using System.Text.Json.Serialization;

namespace PlantBasedPizza.FitnessFunctions.ViewModels
{
    public class AssignDriverRequest
    {
        [JsonPropertyName("orderIdentifier")]
        public string OrderIdentifier { get; set; }
        
        [JsonPropertyName("driverName")]
        public string DriverName { get; set; }
    }
}