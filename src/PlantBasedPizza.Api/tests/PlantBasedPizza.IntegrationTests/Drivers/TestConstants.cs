using System;

namespace PlantBasedPizza.IntegrationTests.Drivers
{
    public static class TestConstants
    {
        public static string DefaultTestUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? "https://public-pizza-api.dev.plantbasedpizza.net";
    }
}