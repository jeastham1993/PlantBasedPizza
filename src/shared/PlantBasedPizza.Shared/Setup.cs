using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using PlantBasedPizza.Shared.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.With(new DataDogLogEnricher())
                .WriteTo.Console(new CompactJsonFormatter());

            if (Environment.GetEnvironmentVariable("TRACE_LOGS") != "Y")
            {
                loggerConfiguration.Filter.ByExcluding(Matching.FromSource("Microsoft"));
                loggerConfiguration.Filter.ByExcluding(Matching.FromSource("System.Net"));
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            services
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
                        // Skip collection of AWS SDK calls or health check calls to other services
                        if (req.RequestUri.ToString().Contains("aws") || req.RequestUri.ToString().Contains("health") || req.RequestUri.ToString().Contains("127.0.0.1"))
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

            return services;
        }
    }
}