using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.DotNet;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SSM;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Amazon.SimpleSystemsManagement.Model;
using AWS.Lambda.Powertools.Parameters;
using Constructs;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;

namespace Infra;

public record LambdaApiRoute(string Path, HttpMethod HttpMethod, string FunctionHandler, string FunctionName);

public class AccountApiInfraStack : Stack
{
    private string _env = System.Environment.GetEnvironmentVariable("ENV") ?? "test";
    private string _serviceName = "AccountApi";
    private string _commitHash = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? "latest";

    internal AccountApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var parameterProvider = ParametersManager.SsmProvider
            .ConfigureClient(System.Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                System.Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
                System.Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));

        HttpApi? api = null;

        try
        {
            var httpApiId = parameterProvider.Get("/shared/api-id");
            api = HttpApi.FromHttpApiAttributes(this, "HttpApi", new HttpApiAttributes
            {
                HttpApiId = httpApiId
            }) as HttpApi;
        }
        catch (ParameterNotFoundException)
        {
            api = new HttpApi(this, "AccountHttpApi", new HttpApiProps
            {
                ApiName = $"AccountApi-{_env}",
                CorsPreflight = new CorsPreflightOptions
                {
                    AllowHeaders = new[] { "*" },
                    AllowMethods = new[]
                        { CorsHttpMethod.POST, CorsHttpMethod.PUT, CorsHttpMethod.DELETE, CorsHttpMethod.GET },
                    AllowOrigins = new[] { "*" }
                }
            });
        }

        var dynamoDbTable = new Table(this, "AccountTable", new TableProps
        {
            PartitionKey = new Attribute
            {
                Name = "PK",
                Type = AttributeType.STRING
            },
            RemovalPolicy = RemovalPolicy.DESTROY,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            TableName = $"UserAccounts-{_env}",
            Stream = StreamViewType.NEW_AND_OLD_IMAGES
        });

        CreateLambdaApplication(api, dynamoDbTable);
    }

    internal void CreateLambdaApplication(HttpApi api, Table table)
    {
        var ddApiKeySecret = Secret.FromSecretNameV2(this, "DDApiKey", "shared/dd-api-key");
        var jwtKeyParam = StringParameter.FromSecureStringParameterAttributes(this, "JWTKeyParam",
            new SecureStringParameterAttributes
            {
                ParameterName = "/shared/jwt-key"
            });
        var ddExtensionLayer = LayerVersion.FromLayerVersionArn(this, "DDExtension",
            $"arn:aws:lambda:{System.Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1"}:464622532012:layer:Datadog-Extension-ARM:77");
        var ddTraceLayer = LayerVersion.FromLayerVersionArn(this, "DDTrace",
            $"arn:aws:lambda:{System.Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1"}:464622532012:layer:dd-trace-dotnet-ARM:20");

        var routes = new List<LambdaApiRoute>();
        routes.Add(new LambdaApiRoute("/account/health", HttpMethod.GET,
            "PlantBasedPizza.Account.Lambda::PlantBasedPizza.Account.Lambda.Functions_Health_Generated::Health",
            $"HealthFunction-{_env}"));
        routes.Add(new LambdaApiRoute("/account/login", HttpMethod.POST,
            "PlantBasedPizza.Account.Lambda::PlantBasedPizza.Account.Lambda.Functions_Login_Generated::Login",
            $"LoginFunction-{_env}"));
        routes.Add(new LambdaApiRoute("/account/register", HttpMethod.POST,
            "PlantBasedPizza.Account.Lambda::PlantBasedPizza.Account.Lambda.Functions_RegisterUser_Generated::RegisterUser",
            $"RegisterUserFunction-{_env}"));
        routes.Add(new LambdaApiRoute("/account/driver/register", HttpMethod.POST,
            "PlantBasedPizza.Account.Lambda::PlantBasedPizza.Account.Lambda.Functions_RegisterDriver_Generated::RegisterDriver",
            $"RegisterDriverFunction-{_env}"));
        routes.Add(new LambdaApiRoute("/account/staff/register", HttpMethod.POST,
            "PlantBasedPizza.Account.Lambda::PlantBasedPizza.Account.Lambda.Functions_RegisterStaff_Generated::RegisterStaff",
            $"RegisterStaffFunction-{_env}"));

        var lambdaProjectPath = "../application/PlantBasedPizza.Account.Lambda/";

        foreach (var route in routes)
        {
            var apiHandlerFunction = new DotNetFunction(this, $"{route.FunctionName}",
                new DotNetFunctionProps
                {
                    ProjectDir = lambdaProjectPath,
                    Handler = route.FunctionHandler,
                    MemorySize = 1024,
                    Timeout = Duration.Seconds(29),
                    Runtime = Runtime.DOTNET_8,
                    Environment = new Dictionary<string, string>
                    {
                        { "DynamoDB__TableName", table.TableName },
                        { "Auth__Issuer", "https://plantbasedpizza.com" },
                        { "Auth__Audience", "https://plantbasedpizza.com" },
                        { "Auth__Key", "This is a sample secret key - please don't use in production environment.'" },
                        { "AWS_LAMBDA_EXEC_WRAPPER", "/opt/datadog_wrapper" },
                        { "DD_ENV", _env },
                        { "DD_SERVICE", _serviceName },
                        { "DD_VERSION", _commitHash },
                        { "DD_API_KEY_SECRET_ARN", ddApiKeySecret.SecretArn },
                        { "DD_SITE", "datadoghq.eu" },
                        { "DD_CAPTURE_LAMBDA_PAYLOAD", "true" }
                    },
                    Architecture = Architecture.ARM_64,
                    FunctionName = route.FunctionName,
                    LogRetention = RetentionDays.ONE_DAY,
                    Layers =
                    [
                        ddExtensionLayer, ddTraceLayer
                    ]
                });
            table.GrantReadWriteData(apiHandlerFunction);
            ddApiKeySecret.GrantRead(apiHandlerFunction);
            api.AddRoutes(new AddRoutesOptions
            {
                Path = route.Path,
                Methods = new[] { route.HttpMethod },
                Integration = new HttpLambdaIntegration("LoginFunctionIntegration", apiHandlerFunction)
            });
        }

        var userCreatedTopic = new Topic(this, "UserCreatedTopic");

        var outboxFunction = new DotNetFunction(this, "OutboxFunction",
            new DotNetFunctionProps
            {
                ProjectDir = lambdaProjectPath,
                Handler =
                    "PlantBasedPizza.Account.Lambda::PlantBasedPizza.Account.Lambda.Outbox_ProcessStream_Generated::ProcessStream",
                MemorySize = 1024,
                Timeout = Duration.Seconds(120),
                Runtime = Runtime.DOTNET_8,
                Environment = new Dictionary<string, string>
                {
                    { "DynamoDB__TableName", table.TableName },
                    { "SNS__UserCreatedTopicArn", userCreatedTopic.TopicArn },
                    { "AWS_LAMBDA_EXEC_WRAPPER", "/opt/datadog_wrapper" },
                    { "DD_ENV", _env },
                    { "DD_SERVICE", _serviceName },
                    { "DD_VERSION", _commitHash },
                    { "DD_API_KEY_SECRET_ARN", ddApiKeySecret.SecretArn },
                    { "DD_SITE", "datadoghq.eu" },
                    { "DD_CAPTURE_LAMBDA_PAYLOAD", "true" }
                },
                Architecture = Architecture.ARM_64,
                FunctionName = $"OutboxFunction-{_env}",
                LogRetention = RetentionDays.ONE_DAY,
                Layers =
                [
                    ddExtensionLayer, ddTraceLayer
                ]
            });
        table.GrantStreamRead(outboxFunction);
        ddApiKeySecret.GrantRead(outboxFunction);
        userCreatedTopic.GrantPublish(outboxFunction);
        outboxFunction.AddEventSource(new DynamoEventSource(table, new DynamoEventSourceProps()));
    }
}