using PlantBasedPizza.Events;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var applicationName = "RecipesApi";

builder.Services.AddRecipeInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration, applicationName)
    .AddMessaging(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
