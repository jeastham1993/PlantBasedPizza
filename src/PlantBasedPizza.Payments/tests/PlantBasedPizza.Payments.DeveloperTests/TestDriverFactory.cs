using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;

namespace PlantBasedPizza.Payments.InMemoryTests;

public static class TestDriverFactory
{
    public static ITestDriver LoadTestDriver(InMemoryEventPublisher eventPublisher, MemoryDistributedCache cache)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("INTEGRATION_TEST")))
        {
            return new IntegrationTestDriver();
        }

        return new TestDriver(eventPublisher, cache);
    }
}