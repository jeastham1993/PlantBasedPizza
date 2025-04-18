using System.Text;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.Account.Core;
using PlantBasedPizza.Account.Core.Adapters;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add DynamoDB configuration
builder.Services.AddSingleton<IAmazonDynamoDB>(sp => 
{
    var environment = builder.Environment.EnvironmentName;
    var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase) || 
                        environment.Equals("local", StringComparison.OrdinalIgnoreCase);
    
    if (isDevelopment && !string.IsNullOrEmpty(builder.Configuration["DynamoDB:LocalEndpoint"]))
    {
        // Use DynamoDB Local for development
        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = builder.Configuration["DynamoDB:LocalEndpoint"] ?? "http://localhost:8000"
        };

        var client = new AmazonDynamoDBClient(new BasicAWSCredentials("dummy", "dummy"), config);
        
        // Check to see if the table exists in DynamoDB local and if not create it.
        try
        {
            client.CreateTableAsync(new CreateTableRequest()
            {
                TableName = builder.Configuration["DynamoDB:TableName"],
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "PK",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "PK",
                        KeyType = "HASH" // Partition key
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST,
            }).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        // For local development, use dummy credentials
        return client;
    }
    else
    {
        // Use actual AWS DynamoDB for non-development environments
        var region = builder.Configuration["AWS:Region"] ?? RegionEndpoint.USEast1.SystemName;
        return new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region));
    }
});

// Configure table name
builder.Services.AddOptions<DynamoDbOptions>()
    .Configure(options => 
    {
        options.TableName = builder.Configuration["DynamoDB:TableName"] ?? "PlantBasedPizza-Accounts";
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Auth:Issuer"],
        ValidAudience = builder.Configuration["Auth:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Auth:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddSerilog();

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IUserAccountRepository, DynamoDbUserAccountRepository>();
builder.Services.AddSingleton<UserAccountService>();
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Auth"));

builder.Services.AddSharedInfrastructure(builder.Configuration, builder.Configuration["SERVICE_NAME"])
    .AddMessaging(builder.Configuration);

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();


var accountRepository = app.Services.GetRequiredService<IUserAccountRepository>();
var userAccountService = app.Services.GetRequiredService<UserAccountService>();

await accountRepository.SeedInitialUser();

app.MapGet("/account/health", () => Task.FromResult("OK")).RequireCors("CorsPolicy");

app.MapPost("/account/login", [AllowAnonymous] async (LoginCommand login) =>
{
    try
    {
        var loginResponse = await userAccountService.Login(login);
        return Results.Ok(loginResponse);
    }
    catch (LoginFailedException)
    {
        return Results.Unauthorized();
    }
}).RequireCors("CorsPolicy");

app.MapPost("/account/register", [AllowAnonymous] async (RegisterUserCommand register) =>
{
    try
    {
        var userAccount = await userAccountService.Register(register, AccountType.User);
        return Results.Ok(new RegisterResponse { AccountId = userAccount.AccountId });
    }
    catch (UserExistsException)
    {
        return Results.BadRequest("User exists");
    }
}).RequireCors("CorsPolicy");

app.MapPost("/account/driver/register", [AllowAnonymous] async (RegisterUserCommand register) =>
{
    try
    {
        var userAccount = await userAccountService.Register(register, AccountType.Driver);
        return Results.Ok(new RegisterResponse { AccountId = userAccount.AccountId });
    }
    catch (UserExistsException)
    {
        return Results.BadRequest("User exists");
    }
}).RequireCors("CorsPolicy");

app.MapPost("/account/staff/register", [AllowAnonymous] async (RegisterUserCommand register) =>
{
    try
    {
        var userAccount = await userAccountService.Register(register, AccountType.Staff);
        
        return Results.Ok(new RegisterResponse()
        {
            AccountId = userAccount.AccountId
        });
    }
    catch (UserExistsException)
    {
        return Results.BadRequest("User exists");
    }
}).RequireAuthorization(policyBuilder => policyBuilder.RequireRole("admin")).RequireCors("CorsPolicy");

// Setup DynamoDB table if in development mode
var isDevelopment = builder.Environment.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
                   builder.Environment.EnvironmentName.Equals("local", StringComparison.OrdinalIgnoreCase);
var usingDynamoLocal = !string.IsNullOrEmpty(builder.Configuration["DynamoDB:LocalEndpoint"]);

if (isDevelopment && usingDynamoLocal)
{
    try
    {
        var dynamoDbClient = app.Services.GetRequiredService<IAmazonDynamoDB>();
        var tableOptions = app.Services.GetRequiredService<IOptions<DynamoDbOptions>>().Value;
        
        // Check if table exists
        var tableExists = false;
        try
        {
            var tableResponse = await dynamoDbClient.DescribeTableAsync(tableOptions.TableName);
            tableExists = true;
        }
        catch (ResourceNotFoundException)
        {
            // Table doesn't exist, we'll create it
            tableExists = false;
        }
        
        if (!tableExists)
        {
            // Create the table with email address as partition key
            var createTableRequest = new CreateTableRequest
            {
                TableName = tableOptions.TableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "PK",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "PK",
                        KeyType = "HASH" // Partition key
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };
            
            await dynamoDbClient.CreateTableAsync(createTableRequest);
            Console.WriteLine($"Created DynamoDB table: {tableOptions.TableName}");
            
            // Wait for table to be active
            bool isTableActive = false;
            while (!isTableActive)
            {
                var tableStatus = await dynamoDbClient.DescribeTableAsync(tableOptions.TableName);
                isTableActive = tableStatus.Table.TableStatus == TableStatus.ACTIVE;
                
                if (!isTableActive)
                {
                    await Task.Delay(1000); // Wait 1 second before checking again
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error setting up DynamoDB table: {ex.Message}");
    }
}

app.Run();
