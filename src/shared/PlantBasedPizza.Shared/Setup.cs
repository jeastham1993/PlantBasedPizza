using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using PlantBasedPizza.Shared.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlantBasedPizza.Shared.ServiceDiscovery;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace PlantBasedPizza.Shared
{
    public static class Setup
    {
        private const string OTEL_DEFAULT_GRPC_ENDPOINT = "http://localhost:4317";
        
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string applicationName)
        {
            ApplicationLogger.Init();
            Log.Logger = new LoggerConfiguration()
                .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                .Enrich.With(new DataDogLogEnricher())
                .WriteTo.Console(new CompactJsonFormatter())
                .CreateLogger();

            services
                .AddLogging()
                .AddSerilog();
            
            var otel = services.AddOpenTelemetry();

            var metadataUri =
                Environment.GetEnvironmentVariable("ECS_CONTAINER_METADATA_URI_V4");

            var taskId = Environment.MachineName;

            if (!string.IsNullOrEmpty(metadataUri))
            {
                Log.Information($"Metadata URI: {metadataUri}");
                Console.WriteLine(metadataUri);
                
                taskId = metadataUri.Split("/").Last()
                    .Split("-").First();

                var httpClient = new HttpClient();
                var getEcsMetadata = httpClient.GetAsync(metadataUri).GetAwaiter().GetResult();
                Log.Information(getEcsMetadata.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                Console.WriteLine(getEcsMetadata.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }

            otel.ConfigureResource(resource => resource
                .AddService(serviceName: applicationName)
                .AddAttributes(new List<KeyValuePair<string, object>>(2)
                {
                    new("service.name", applicationName),
                    new("container.id", taskId)
                }));
            
            otel.WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = (context) =>
                    {
                        if (context.Request.Path.Value.Contains("/health") || context.Request.Path.Value == "/")
                        {
                            return false;
                        }

                        return true;
                    };
                });
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