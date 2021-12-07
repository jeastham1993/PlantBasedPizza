namespace PlantBasedPizza.Deliver.Core.Commands
{
    public class AssignDriverRequest
    {
        public string OrderIdentifier { get; set; }
        
        public string DriverName { get; set; }
    }
}