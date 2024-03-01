using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Shared.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PlantBasedPizza.Shared
{
    using OpenTelemetry.Exporter;

    public static class Setup
    {
        private const string APPLICATION_NAME = "PlantBasedPizza";
        
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            ApplicationLogger.Init();
            
            var otel = services.AddOpenTelemetry();
            otel.ConfigureResource(resource => resource
                .AddService(serviceName: APPLICATION_NAME));
            
            otel.WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddSource(APPLICATION_NAME);
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(configuration["OtlpEndpoint"]);
                    otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
                tracing.AddConsoleExporter();
            });

            services.AddSingleton<IObservabilityService, ObservabiityService>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}