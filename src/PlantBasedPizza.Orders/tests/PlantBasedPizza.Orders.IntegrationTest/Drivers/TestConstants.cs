namespace PlantBasedPizza.Orders.IntegrationTest.Drivers
{
    public static class TestConstants
    {
        public static string DefaultTestUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? "http://localhost:5004";
    }
}