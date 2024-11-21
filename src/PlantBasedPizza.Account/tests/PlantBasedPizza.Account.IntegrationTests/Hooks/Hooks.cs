using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TechTalk.SpecFlow;

namespace PlantBasedPizza.Account.IntegrationTests.Hooks;

[Binding]
public static class Hook
{
    private const string ServiceName = "PlantBasedPizzaAccountIntegrationTests";

    public static ActivitySource Source { get; private set; } = new ActivitySource(ServiceName);
    public static TracerProvider? TracerProvider { get; private set; }
    
    public static Activity? CurrentActivity { get; private set; }

    public static Activity RootActivity { get; private set; } = Source.StartActivity("integration-test-run")!;
    
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(ServiceName);

        var traceConfig = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource(ServiceName)
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(ServiceName)
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTLP_ENDPOINT") ?? "http://localhost:4317");
            });

        TracerProvider = traceConfig.Build();
    }

    [BeforeScenario]
    public static void BeforeScenario(ScenarioContext scenarioContext)
    {
        CurrentActivity = Source.StartActivity(scenarioContext.ScenarioInfo.Title, ActivityKind.Client, RootActivity.Context)!;
        
        scenarioContext.Add("Activity", CurrentActivity);
    }

    [AfterScenario]
    public static void AfterScenario()
    {
        CurrentActivity?.Stop();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        RootActivity?.Stop();
        TracerProvider?.ForceFlush();
    }
}