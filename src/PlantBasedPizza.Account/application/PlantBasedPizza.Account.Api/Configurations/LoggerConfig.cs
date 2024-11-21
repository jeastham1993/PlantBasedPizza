using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace PlantBasedPizza.Account.Api.Configurations;

public static class LoggerConfigs
{
    public static WebApplicationBuilder AddLoggerConfigs(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((_, config) =>
        {
            config.MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter());
        });

        return builder;
    }
}