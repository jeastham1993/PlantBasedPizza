using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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

builder.Services.AddAuthorization();

var client = new MongoClient(builder.Configuration["DatabaseConnection"]);

builder.Services.AddSingleton(client);
builder.Services.AddSingleton<IUserAccountRepository, UserAccountRepository>();
            
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

await accountRepository.SeedInitialUser();

app.MapPost("/account/login", [AllowAnonymous] async (LoginCommand login) =>
{
    try
    {
        var account = await accountRepository.ValidateCredentials(login.EmailAddress, login.Password);
    
        var issuer = builder.Configuration["Auth:Issuer"];
        var audience = builder.Configuration["Auth:Audience"];
        var key = Encoding.ASCII.GetBytes
            (builder.Configuration["Auth:Key"]);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.AccountId),
            new Claim(JwtRegisteredClaimNames.Email, account.EmailAddress),
            new Claim(ClaimTypes.Role, account.AsAuthenticatedRole())
        };
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        var stringToken = tokenHandler.WriteToken(token);
    
        return Results.Ok(new LoginResponse()
        {
            AuthToken = stringToken
        });
    }
    catch (LoginFailedException)
    {
        Activity.Current.AddTag("login.failed", true);
        
        return Results.Unauthorized();
    }
});

app.MapPost("/account/register", [AllowAnonymous] async (RegisterUserCommand register) =>
{
    try
    {
        var userAccount = await accountRepository.CreateAccount(register.EmailAddress, register.Password);
        
        return Results.Ok(new RegisterResponse()
        {
            AccountId = userAccount.AccountId
        });
    }
    catch (UserExistsException)
    {
        Activity.Current.AddTag("user.exists", true);
        return Results.BadRequest("User exists");
    }
});

app.MapPost("/account/driver/register", [AllowAnonymous] async (RegisterUserCommand register) =>
{
    try
    {
        var userAccount = await accountRepository.CreateDriverAccount(register.EmailAddress, register.Password);
        
        return Results.Ok(new RegisterResponse()
        {
            AccountId = userAccount.AccountId
        });
    }
    catch (UserExistsException)
    {
        Activity.Current.AddTag("user.exists", true);
        return Results.BadRequest("User exists");
    }
}).RequireAuthorization(policyBuilder => policyBuilder.RequireRole(["staff","admin"]));

app.MapPost("/account/staff/register", async (RegisterUserCommand register) =>
{
    try
    {
        var userAccount = await accountRepository.CreateStaffAccount(register.EmailAddress, register.Password);
        
        return Results.Ok(new RegisterResponse()
        {
            AccountId = userAccount.AccountId
        });
    }
    catch (UserExistsException)
    {
        Activity.Current.AddTag("user.exists", true);
        return Results.BadRequest("User exists");
    }
}).RequireAuthorization(policyBuilder => policyBuilder.RequireRole("admin"));

app.Run();
