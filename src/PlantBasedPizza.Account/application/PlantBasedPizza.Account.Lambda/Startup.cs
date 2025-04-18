using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Account.Core;
using PlantBasedPizza.Account.Core.Adapters;

namespace PlantBasedPizza.Account.Lambda;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    /// <summary>
    /// Services for Lambda functions can be registered in the services dependency injection container in this method. 
    ///
    /// The services can be injected into the Lambda function through the containing type's constructor or as a
    /// parameter in the Lambda function using the FromService attribute. Services injected for the constructor have
    /// the lifetime of the Lambda compute container. Services injected as parameters are created within the scope
    /// of the function invocation.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        // Add configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // Configure JWT settings
        services.Configure<JwtConfiguration>(configuration.GetSection("Auth"));

        // Configure DynamoDB 
        services.Configure<DynamoDbOptions>(options =>
        {
            options.TableName = configuration["DynamoDB:TableName"] ?? "PlantBasedPizza-Accounts";
        });

        // Add DynamoDB client
        services.AddSingleton<IAmazonDynamoDB>(sp =>
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDevelopment = environment?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true ||
                                environment?.Equals("local", StringComparison.OrdinalIgnoreCase) == true;

            if (isDevelopment && !string.IsNullOrEmpty(configuration["DynamoDB:LocalEndpoint"]))
            {
                // Use DynamoDB Local for development
                var config = new AmazonDynamoDBConfig
                {
                    ServiceURL = configuration["DynamoDB:LocalEndpoint"] ?? "http://localhost:8000"
                };

                return new AmazonDynamoDBClient(new BasicAWSCredentials("dummy", "dummy"), config);
            }
            else
            {
                // Use actual AWS DynamoDB for non-development environments
                return new AmazonDynamoDBClient();
            }
        });

        services.AddSingleton(new AmazonSimpleNotificationServiceClient());

        // Add repository and service
        services.AddSingleton<IUserAccountRepository, DynamoDbUserAccountRepository>();
        services.AddSingleton<IEventPublisher, SnsEventPublisher>();
        services.AddSingleton<UserAccountService>();
    }
}