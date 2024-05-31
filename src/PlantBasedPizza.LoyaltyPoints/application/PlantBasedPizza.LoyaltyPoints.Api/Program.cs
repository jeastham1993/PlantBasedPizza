using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlantBasedPizza.LoyaltyPoints;
using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.LoyaltyPoints.Shared.Core;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddAuthorization();

builder.Services.AddLoyaltyServices(builder.Configuration, "LoyaltyPointsAPI");

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

var loyaltyRepo = app.Services.GetRequiredService<ICustomerLoyaltyPointsRepository>();

app.MapGet("/loyalty/health", () => "");

app.MapGet("/loyalty", async (ClaimsPrincipal user) =>
{
    var accountId = user.Claims.ExtractAccountId();
    
    Activity.Current?.AddTag("loyalty.customerId", accountId);
    
    var loyalty = await loyaltyRepo.GetCurrentPointsFor(accountId);

    if (loyalty == null)
    {
        return Results.Ok(new LoyaltyPointsDto(0));
    }

    return Results.Ok(new LoyaltyPointsDto(loyalty));
}).RequireAuthorization(policyBuilder => policyBuilder.RequireRole("user")).RequireCors("CorsPolicy");

app.Run();
