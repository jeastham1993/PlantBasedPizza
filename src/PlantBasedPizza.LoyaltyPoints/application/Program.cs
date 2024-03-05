using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddSharedInfrastructure(builder.Configuration, "LoyaltyPoints");

var app = builder.Build();

app.MapGet("/loyalty/health", () => "");

app.Run();
