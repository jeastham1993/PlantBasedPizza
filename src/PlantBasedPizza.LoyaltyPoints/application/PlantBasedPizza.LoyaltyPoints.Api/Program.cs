using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantBasedPizza.LoyaltyPoints;
using PlantBasedPizza.LoyaltyPoints.Adapters;
using PlantBasedPizza.LoyaltyPoints.Core;
using PlantBasedPizza.LoyaltyPoints.Shared;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLoyaltyServices(builder.Configuration);

var app = builder.Build();

var addLoyaltyPointsHandler = app.Services.GetRequiredService<AddLoyaltyPointsCommandHandler>();
var spendLoyaltyPointsHandler = app.Services.GetRequiredService<SpendLoyaltyPointsCommandHandler>();
var loyaltyRepo = app.Services.GetRequiredService<ICustomerLoyaltyPointsRepository>();

app.MapGet("/loyalty/health", () => "");

app.MapPost("/loyalty", async ([FromBody] AddLoyaltyPointsCommand command) =>
{
    command.AddToTrace();

    if (!command.Validate())
    {
        return Results.BadRequest();
    }
    
    return Results.Ok(await addLoyaltyPointsHandler.Handle(command));
});

app.MapPost("/loyalty/spend", async ([FromBody] SpendLoyaltyPointsCommand command) =>
{
    command.AddToTrace();
    
    return await spendLoyaltyPointsHandler.Handle(command);
});

app.MapGet("/loyalty/{customerIdentifier}", async (string customerIdentifier) =>
{
    Activity.Current?.AddTag("loyalty.customerId", customerIdentifier);
    
    var loyalty = await loyaltyRepo.GetCurrentPointsFor(customerIdentifier);

    if (loyalty == null)
    {
        return Results.NotFound(customerIdentifier);
    }

    return Results.Ok(new LoyaltyPointsDTO(loyalty));
});

app.Run();
