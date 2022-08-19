using System;

namespace PlantBasedPizza.IntegrationTests.Drivers
{
    public static class TestConstants
    {
        public static string DefaultTestUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? "https://bopjd32mx9.execute-api.eu-west-1.amazonaws.com/";
    }
}