using System.Diagnostics;
using Datadog.Trace;
using Datadog.Trace.Configuration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TechTalk.SpecFlow;
using Tracer = Datadog.Trace.Tracer;

namespace PlantBasedPizza.Orders.IntegrationTest.Hooks;

[Binding]
public static class Hook
{
    private const string SERVICE_NAME = "OrdersIntegrationTests";
    
    public static ActivitySource Source { get; private set; }
    public static TracerProvider TracerProvider { get; private set; }
    
    public static IScope Scope { get; private set; }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Tracer.Configure(new TracerSettings());
    }

    [BeforeScenario]
    public static void BeforeScenario(ScenarioContext scenarioContext)
    {
        Scope = Tracer.Instance.StartActive(scenarioContext.ScenarioInfo.Title);
        
        scenarioContext.Add("TraceScope", Scope);
    }

    [AfterScenario]
    public static void AfterScenario()
    {
        Scope.Dispose();
    }
}