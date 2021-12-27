using System;

namespace PlantBasedPizza.IntegrationTests.Drivers
{
    public static class TestConstants
    {
        public static string DefaultTestUrl = Environment.GetEnvironmentVariable("TEST_URL") ?? "http://plant-Publi-VE47TJF21ZGX-799634284.eu-west-1.elb.amazonaws.com";
    }
}