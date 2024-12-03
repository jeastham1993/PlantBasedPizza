using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Orders.IntegrationTest.Hooks;

[Binding]
public static class Hook
{
    private const string SERVICE_NAME = "OrderWorkerIntegrationTest";
    
    public static ActivitySource Source { get; private set; }
    public static TracerProvider TracerProvider { get; private set; }
    
    public static Activity CurrentActivity { get; private set; }
    
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(SERVICE_NAME);

        var traceConfig = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource(SERVICE_NAME)
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(SERVICE_NAME)
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTLP_ENDPOINT") ?? "http://localhost:4317");
            });

        TracerProvider = traceConfig.Build();
        Source = new ActivitySource(SERVICE_NAME);
    }

    [BeforeScenario]
    public static void BeforeScenario(ScenarioContext scenarioContext)
    {
        CurrentActivity = Source.StartActivity(scenarioContext.ScenarioInfo.Title, ActivityKind.Client);
        
        scenarioContext.Add("Activity", CurrentActivity);
    }

    [AfterScenario]
    public static void AfterScenario()
    {
        CurrentActivity.Stop();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        TracerProvider.ForceFlush();
    }
}