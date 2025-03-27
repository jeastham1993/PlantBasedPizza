using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var applicationName = "RecipesApi";

builder.Services.AddRecipeInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration, applicationName);

builder.Services.AddHttpClient();

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();