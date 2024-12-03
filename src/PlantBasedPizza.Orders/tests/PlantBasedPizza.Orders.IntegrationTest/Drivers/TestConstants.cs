namespace PlantBasedPizza.Orders.IntegrationTest.Drivers
{
    public static class TestConstants
    {
        public static string DefaultTestUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? "http://localhost:5004";
        
        public static string DefaultMongoDbConnection = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION") ?? "mongodb://localhost:27017";
    }
}