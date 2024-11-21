using PlantBasedPizza.Payments;
using PlantBasedPizza.Payments.Services;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);
builder
    .Configuration
    .AddEnvironmentVariables();
builder.AddLoggerConfigs();

// Add services to the container.
builder.Services.AddGrpc();

builder.Services
    .AddSharedInfrastructure(builder.Configuration, "PaymentApi");

builder.Services.AddDaprClient();

builder.Services.AddSingleton<APIKeyProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.MapGrpcService<PaymentService>();
app.MapGet("/payments/health", () => "Healthy");

app.Run();