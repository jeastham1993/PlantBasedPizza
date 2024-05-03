using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
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
                    var otlpEndpoint = configuration["OtlpEndpoint"];
                    var otlpUseHttp = configuration["OtlpUseHttp"];
                    
                    otlpOptions.Endpoint = new Uri(otlpEndpoint ?? OTEL_DEFAULT_GRPC_ENDPOINT);
                    otlpOptions.Protocol = otlpUseHttp == "Y" ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
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
                
                services.AddSingleton<IServiceRegistry, ConsulServiceRegistry>();
            }

            // Only register the running application with Consul if it has a valid URL
            var myUrl = configuration.GetSection("ServiceDiscovery")["MyUrl"];
            if (!string.IsNullOrEmpty(myUrl))
            {
                services.AddSingleton<IHostedService, ConsulRegisterService>();   
            }

            services.Configure<ServiceDiscoverySettings>(configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<ServiceRegistryHttpMessageHandler>();

            return services;
        }
    }
}