using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Shared.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PlantBasedPizza.Shared
{
    public static class Setup
    {
        private const string OTEL_DEFAULT_GRPC_ENDPOINT = "http://localhost:4317";
        
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string applicationName)
        {
            ApplicationLogger.Init();
            
            var otel = services.AddOpenTelemetry();
            otel.ConfigureResource(resource => resource
                .AddService(serviceName: applicationName));
            
            otel.WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddSource(applicationName);
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(configuration["OtlpEndpoint"] ?? OTEL_DEFAULT_GRPC_ENDPOINT);
                });
            });

            services.AddSingleton<IObservabilityService, ObservabiityService>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}