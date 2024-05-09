using System.Net.Http.Json;
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
            EcsMetadata? metadata = null;

            if (!string.IsNullOrEmpty(metadataUri))
            {
                Log.Information("Retrieving ECS Metadata");
                
                var httpClient = new HttpClient();
                metadata = httpClient.GetFromJsonAsync<EcsMetadata>(metadataUri).GetAwaiter().GetResult();
                
                Log.Information($"Retrieve, DockerID is {metadata.DockerId}");
                
                taskId = metadata.DockerId;
                httpClient.Dispose();
            }

            otel.ConfigureResource(resource =>
            {
                resource
                    .AddService(serviceName: applicationName)
                    .AddAttributes(new List<KeyValuePair<string, object>>()
                    {
                        new("service.name", applicationName),
                        new("container.id", taskId ?? ""),
                        new("service.version", Environment.GetEnvironmentVariable("DD_VERSION") ?? ""),
                    });

                if (metadata != null)
                {
                    Log.Information("Adding metadata attributes to resource");
                    
                    resource.AddAttributes(new List<KeyValuePair<string, object>>()
                    {
                        new("ecs.cluster.name", metadata.Name ?? ""),
                        new("ecs.task.arn", metadata.ContainerARN ?? ""),
                        new("ecs.cpu.limit", metadata.Limits.CPU.ToString()),
                        new("ecs.taskDefinition.family", metadata.Labels.TaskDefinitionFamily ?? ""),
                        new("ecs.taskDefinition.version", metadata.Labels.TaskDefinitionVersion ?? ""),
                        new("container.image", metadata.Image ?? ""),
                        new("container.startedAt", metadata.StartedAt ?? ""),
                        new("container.createdAt", metadata.CreatedAt ?? ""),
                    });
                }
            });
            
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
                    options.RecordException = true;
                });
                tracing.AddGrpcClientInstrumentation(options =>
                {
                });
                tracing.AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.FilterHttpRequestMessage = (req) =>
                    {
                        // Skip collection of AWS SDK calls
                        if (req.RequestUri.ToString().Contains("aws"))
                        {
                            return false;
                        }

                        return true;
                    };
                });
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