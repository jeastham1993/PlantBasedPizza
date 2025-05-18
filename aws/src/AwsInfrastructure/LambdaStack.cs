using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.DotNet;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace AwsInfrastructure;

public class LambdaStack : Stack
{
    internal LambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var connectionStringParameter = new StringParameter(this, "ConnectionStringParameter", new StringParameterProps()
        {
            ParameterName = "PlantBasedPizzaPostgresConnection",
            StringValue = System.Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? throw new ArgumentException("CONNECTION_STRING environment variable is not set"),
        });
        
        var standardFunction = new DotNetFunction(this, "PlantBasedPizzaMonolithBase", new DotNetFunctionProps()
        {
            ProjectDir = "../src/PlantBasedPizza.Api/application/PlantBasedPizza.Api/",
            Handler = "PlantBasedPizza.Api",
            MemorySize = 2048,
            Timeout = Duration.Seconds(30),
            Architecture = Architecture.X86_64,
            Runtime = Runtime.DOTNET_8,
            FunctionName = "PlantBasedPizzaMonolith-Base",
            LogRetention = RetentionDays.ONE_DAY,
            Environment = new Dictionary<string, string>(1)
            {
                {"ConnectionStringParameterName", connectionStringParameter.ParameterName}
            }
        });
        connectionStringParameter.GrantRead(standardFunction.Role);
        
        var version = System.Environment.GetEnvironmentVariable("COMMIT_HASH") ?? DateTime.Now.ToString("yyyyMMddhhmmss");

        var snapStartFunction = new DotNetFunction(this, "PlantBasedPizzaMonolithSnapStart", new DotNetFunctionProps()
        {
            ProjectDir = "../src/PlantBasedPizza.Api/application/PlantBasedPizza.Api/",
            Handler = "PlantBasedPizza.Api",
            MemorySize = 2048,
            Timeout = Duration.Seconds(30),
            Architecture = Architecture.X86_64,
            Runtime = Runtime.DOTNET_8,
            FunctionName = "PlantBasedPizzaMonolith-SnapStart",
            LogRetention = RetentionDays.ONE_DAY,
            SnapStart = SnapStartConf.ON_PUBLISHED_VERSIONS,
            CurrentVersionOptions = new VersionOptions()
            {
                Description = version
            },
            Environment = new Dictionary<string, string>(1)
            {
                {"ConnectionStringParameterName", connectionStringParameter.ParameterName}
            }
        });
        connectionStringParameter.GrantRead(snapStartFunction.Role);
        var productionAlias = new Alias(this, "ProductionAlias", new AliasProps
        {
            AliasName = "production",
            Version = snapStartFunction.CurrentVersion,
        });
        productionAlias.AddFunctionUrl(new FunctionUrlOptions()
        {
            AuthType = FunctionUrlAuthType.NONE,
            Cors = new FunctionUrlCorsOptions()
            {
                AllowedOrigins = new string[1] { "*" }
            }
        });
        
        var snapStartPrewarmed = new DotNetFunction(this, "PlantBasedPizzaMonolithPreWarm", new DotNetFunctionProps()
        {
            ProjectDir = "../src/PlantBasedPizza.Api/application/PlantBasedPizza.Api/",
            Handler = "PlantBasedPizza.Api",
            MemorySize = 2048,
            Timeout = Duration.Seconds(30),
            Architecture = Architecture.X86_64,
            Runtime = Runtime.DOTNET_8,
            FunctionName = "PlantBasedPizzaMonolith-Prewarmed",
            LogRetention = RetentionDays.ONE_DAY,
            SnapStart = SnapStartConf.ON_PUBLISHED_VERSIONS,
            CurrentVersionOptions = new VersionOptions()
            {
                Description = version
            },
            Environment = new Dictionary<string, string>(1)
            {
                {"PRE_WARM", "Y"},
                {"ConnectionStringParameterName", connectionStringParameter.ParameterName}
            }
        });
        var prewarmedProductionAlias = new Alias(this, "PrewarmedProductionAlias", new AliasProps
        {
            AliasName = "production",
            Version = snapStartPrewarmed.CurrentVersion,
        });
        prewarmedProductionAlias.AddFunctionUrl(new FunctionUrlOptions()
        {
            AuthType = FunctionUrlAuthType.NONE,
            Cors = new FunctionUrlCorsOptions()
            {
                AllowedOrigins = new string[1] { "*" }
            }
        });
        connectionStringParameter.GrantRead(snapStartPrewarmed.Role);
    }
}