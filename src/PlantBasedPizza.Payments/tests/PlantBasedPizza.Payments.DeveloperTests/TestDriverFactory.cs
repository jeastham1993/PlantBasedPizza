using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;

namespace PlantBasedPizza.Payments.InMemoryTests;

public static class TestDriverFactory
{
    public static ITestDriver LoadTestDriver(InMemoryEventPublisher eventPublisher, MemoryDistributedCache cache)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_HARNESS_ENDPOINT")))
        {
            return new IntegrationTestDriver();
        }

        return new TestDriver(eventPublisher, cache);
    }
}