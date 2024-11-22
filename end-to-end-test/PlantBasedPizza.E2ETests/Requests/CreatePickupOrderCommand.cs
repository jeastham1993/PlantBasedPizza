namespace PlantBasedPizza.E2ETests.Requests
{
    public class CreatePickupOrderCommand
    {
        public string CustomerIdentifier { get; set; }

        public int OrderType = 0;
    }
}