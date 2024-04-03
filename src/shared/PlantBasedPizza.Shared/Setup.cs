using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Shared.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlantBasedPizza.Shared.ServiceDiscovery;

namespace PlantBasedPizza.Shared
{
    public static class Setup
    {
        private const string OTEL_DEFAULT_GRPC_ENDPOINT = "http://localhost:4317";
        
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string applicationName)
        {
            ApplicationLogger.Init();

            services.AddLogging();
            
            var otel = services.AddOpenTelemetry();
            otel.ConfigureResource(resource => resource
                .AddService(serviceName: applicationName));
            
            otel.WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddGrpcClientInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddSource(applicationName);
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(configuration["OtlpEndpoint"] ?? OTEL_DEFAULT_GRPC_ENDPOINT);
                });
            });

            services.AddSingleton<IObservabilityService, ObservabiityService>();
            services.AddHttpContextAccessor();

            var consulAddress = configuration.GetSection("ServiceDiscovery")["ConsulServiceEndpoint"];

            if (string.IsNullOrEmpty(consulAddress))
            {
                services.AddSingleton<IServiceRegistry, ConfigurationFileServiceRegistry>();
            }
            else
            {
                services.AddSingleton<IConsulClient, ConsulClient>(provider =>
                    new ConsulClient(config => config.Address = new Uri(consulAddress)));
                
                services.AddSingleton<IHostedService, ConsulRegisterService>();
                services.AddSingleton<IServiceRegistry, ConsulServiceRegistry>();
            }

            services.Configure<ServiceDiscoverySettings>(configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<ServiceRegistryHttpMessageHandler>();

            return services;
        }
    }
}