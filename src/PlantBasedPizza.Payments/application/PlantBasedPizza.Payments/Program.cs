using PlantBasedPizza.Events;
using PlantBasedPizza.Payments;
using PlantBasedPizza.Payments.Services;
using PlantBasedPizza.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services
    .AddSharedInfrastructure(builder.Configuration, "Payments")
    .AddMessaging(builder.Configuration);

builder.Services.AddSingleton<APIKeyProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.MapGrpcService<PaymentService>();
app.MapGet("/payments/health", () => "Healthy");

app.Run();