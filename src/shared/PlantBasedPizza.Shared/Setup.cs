using Amazon;
using Amazon.CloudWatch;
using Amazon.Runtime;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Handlers.System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace PlantBasedPizza.Shared
{
    public static class Setup
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            AWSXRayRecorder.RegisterLogger(LoggingOptions.Console);
            AWSXRayRecorder.InitializeInstance(configuration);
            AWSSDKHandler.RegisterXRayForAllServices();
            
            ApplicationLogger.Init();

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ENV")))
            {
                services.AddSingleton(new AmazonCloudWatchClient(
                    new BasicAWSCredentials(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                        Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")), RegionEndpoint.EUWest1));
            }
            else
            {
                services.AddSingleton(new AmazonCloudWatchClient());
            }
            
            services.AddTransient<IObservabilityService, ObservabiityService>();
            services.AddHttpContextAccessor();

            return services;
        }
        
        public static WebApplicationBuilder AddSharedInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((ctx, lc) => lc
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), "logs/myapp-{Date}.json"));

            return builder;
        }
    }
}