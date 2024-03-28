using Newtonsoft.Json;

namespace PlantBasedPizza.E2ETests.ViewModels
{
    public class AssignDriverRequest
    {
        [JsonProperty("orderIdentifier")]
        public string OrderIdentifier { get; set; }
        
        [JsonProperty("driverName")]
        public string DriverName { get; set; }
    }
}