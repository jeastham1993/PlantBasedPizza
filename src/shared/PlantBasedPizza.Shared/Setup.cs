using Amazon;
using Amazon.CloudWatch;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Handlers.System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Shared.Logging;
using Serilog;

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
            
            Logger.Init();

            services.AddSingleton(new AmazonCloudWatchClient());
            services.AddTransient<IObservabilityService, ObservabiityService>();

            return services;
        }
        
        public static WebApplicationBuilder AddSharedInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console());

            return builder;
        }
    }
}