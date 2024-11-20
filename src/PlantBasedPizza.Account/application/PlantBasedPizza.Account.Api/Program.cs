using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.Account.Api;
using PlantBasedPizza.Account.Api.Adapters;
using PlantBasedPizza.Account.Api.Core;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

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

builder.Services.AddDaprClient();

builder.Services.AddAuthorization();

var client = new MongoClient(builder.Configuration["DatabaseConnection"]);

builder.Services.AddSingleton(client);
builder.Services.AddSingleton<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddSingleton<UserAccountService>();
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Auth"));
            
BsonClassMap.RegisterClassMap<UserAccount>(map =>
{
    map.AutoMap();
    map.SetIgnoreExtraElements(true);
    map.SetIgnoreExtraElementsIsInherited(true);
});

var serviceName = "PlantBasedPizza.Account";

builder.Services.AddSharedInfrastructure(builder.Configuration, serviceName)
    .AddMessaging(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var accountRepository = app.Services.GetRequiredService<IUserAccountRepository>();
var userAccountService = app.Services.GetRequiredService<UserAccountService>();

await accountRepository.SeedInitialUser();

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
});

app.MapPost("/account/register", [AllowAnonymous] async (RegisterUserCommand register) =>
{
    try
    {
        var userAccount = await userAccountService.Register(register, AccountType.User);
        
        return Results.Ok(new RegisterResponse()
        {
            AccountId = userAccount.AccountId
        });
    }
    catch (UserExistsException)
    {
        return Results.BadRequest("User exists");
    }
});

app.MapPost("/account/driver/register", [AllowAnonymous] async (RegisterUserCommand register) =>
{
    try
    {
        var userAccount = await userAccountService.Register(register, AccountType.Driver);
        
        return Results.Ok(new RegisterResponse()
        {
            AccountId = userAccount.AccountId
        });
    }
    catch (UserExistsException)
    {
        return Results.BadRequest("User exists");
    }
}).RequireAuthorization(policyBuilder => policyBuilder.RequireRole(["staff","admin"]));

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
}).RequireAuthorization(policyBuilder => policyBuilder.RequireRole("admin"));

app.Run();
