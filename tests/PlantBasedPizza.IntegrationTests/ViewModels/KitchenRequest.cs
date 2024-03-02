using System;

namespace PlantBasedPizza.IntegrationTests.ViewModels
{
    public class KitchenRequest
    {
        public string KitchenRequestId { get; set; }
        
        public string OrderIdentifier { get; set; }
        
        public DateTime OrderReceivedOn { get; set; }
        
        public DateTime? PrepCompleteOn { get; set; }
        
        public DateTime? BakeCompleteOn { get; set; }
        
        public DateTime? QualithCheckCompleteOn { get; set; }
    }
}