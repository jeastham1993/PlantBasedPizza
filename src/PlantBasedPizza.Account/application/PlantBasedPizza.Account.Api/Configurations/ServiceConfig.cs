using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.Account.Api.Adapters;
using PlantBasedPizza.Account.Api.Core;

namespace PlantBasedPizza.Account.Api.Configurations;

public static class ServiceConfigs
{
    public static IServiceCollection AddServiceConfigs(this IServiceCollection services,
        ILogger logger, WebApplicationBuilder builder)
    {
        var client = new MongoClient(builder.Configuration["DatabaseConnection"]);

        builder.Services.AddSingleton(client);
        builder.Services.AddSingleton<IUserAccountRepository, UserAccountRepository>();
        builder.Services.AddSingleton<UserAccountService>();
        
        BsonClassMap.RegisterClassMap<UserAccount>(map =>
        {
            map.AutoMap();
            map.SetIgnoreExtraElements(true);
            map.SetIgnoreExtraElementsIsInherited(true);
        });
        
        logger.LogInformation("Added services");

        return services;
    }
}