using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlantBasedPizza.Payments.PublicEvents;

namespace PlantBasedPizza.Payments.InMemoryTests;

public static class Setup
{
    public static WebApplicationFactory<Program> StartInMemoryServer(IPaymentEventPublisher eventPublisher, MemoryDistributedCache memoryCache, List<Activity> exportedActivities)
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IPaymentEventPublisher>(eventPublisher);
                    services.AddSingleton<IDistributedCache>(memoryCache);
                    var otel = services.AddOpenTelemetry();
                    otel.ConfigureResource(resource => resource
                        .AddService("PaymentApi"));

                    otel.WithTracing(tracing =>
                    {
                        tracing.AddAspNetCoreInstrumentation();
                        tracing.AddGrpcClientInstrumentation();
                        tracing.AddHttpClientInstrumentation();
                        tracing.AddSource("PaymentApi");

                        tracing.AddInMemoryExporter(exportedActivities);
                    });
                });
            });

        return application;
    }
}