using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlantBasedPizza.Recipes.Infrastructure
{
    public static class Startup
    {
        public static IServiceProvider Services;

        public static void Configure()
        {
            var collection = new ServiceCollection();

            collection.ConfigureDatabase();

            collection.AddTransient<IRecipeRepository, RecipeRepository>();

            Shared.Setup.AddSharedInfrastructure(collection);

            Services = collection.BuildServiceProvider();
        }

        private static IServiceCollection ConfigureDatabase(this IServiceCollection collection)
        {
            Console.WriteLine("Configuring database settings");

            var credentials = LoadDatabaseSecret();

            Console.WriteLine("Loaded database secret");

            var connectionString = credentials.ToString();

            var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString)); 
            
            if (credentials.ssl)
            {
                Console.WriteLine("SSL enabled, overriding settings");

                settings.SslSettings = new SslSettings()
                { 
                    CheckCertificateRevocation = false, 
                    ServerCertificateValidationCallback = (o, c, ch, er) => true, 
                }; 
                
                ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true; 
            }

            var mongoClient = new MongoClient(settings);

            collection.AddSingleton(mongoClient);

            var database = mongoClient.GetDatabase("recipedb");

            Console.WriteLine("Got database");

            BsonClassMap.RegisterClassMap<Recipe>(map =>
            {
                map.AutoMap();
                map.MapField("_ingredients");
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });

            BsonClassMap.RegisterClassMap<Ingredient>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });

            return collection;
        }

        private static DatabaseCredentials LoadDatabaseSecret()
        {
            var client = new AmazonSecretsManagerClient();

            var databaseConnectionSecret = client.GetSecretValueAsync(new GetSecretValueRequest()
            {
                SecretId = Environment.GetEnvironmentVariable("DATABASE_SECRET_ARN"),
            }).Result;

            return JsonSerializer
                .Deserialize<DatabaseCredentials>(databaseConnectionSecret.SecretString);
        }
    }
}
