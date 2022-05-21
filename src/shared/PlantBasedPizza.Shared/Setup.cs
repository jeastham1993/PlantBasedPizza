using Amazon;
using Amazon.CloudWatch;
using Amazon.EventBridge;
using Amazon.Runtime;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Handlers.System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace PlantBasedPizza.Shared
{
    public static class Setup
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
        {
            AWSXRayRecorder.InitializeInstance();
            AWSSDKHandler.RegisterXRayForAllServices();
            
            ApplicationLogger.Init();

            services.AddSingleton(new AmazonCloudWatchClient());
            services.AddSingleton(new AmazonEventBridgeClient());

            services.AddTransient<IObservabilityService, ObservabiityService>();
            services.AddTransient<IEventBus, EventBridgeEventBus>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}