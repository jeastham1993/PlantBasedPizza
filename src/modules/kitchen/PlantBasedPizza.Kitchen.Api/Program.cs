using Amazon.CloudWatch;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.SQS;
using CorrelationId;
using CorrelationId.DependencyInjection;
using PlantBasedPizza.Kitchen.Api;
using PlantBasedPizza.Kitchen.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDefaultCorrelationId();
builder.Services.AddSingleton(new AmazonSQSClient());
builder.Services.AddSingleton<IQueueManager, SqsQueueManager>();
builder.Services.AddSingleton(new AmazonCloudWatchClient());
builder.Services.AddTransient<IMetrics, CloudWatchMetrics>();

var systemsManager = new AmazonSimpleSystemsManagementClient();
builder.Services.AddSingleton(systemsManager);

var activeKitchens = systemsManager.GetParameterAsync(new GetParameterRequest()
{
    Name = builder.Configuration["ActiveLocationsParameterName"]
}).Result.Parameter.Value;

var queueUrls = new Dictionary<string, string>();

foreach (var location in activeKitchens.Split(','))
{
    queueUrls.Add(location, systemsManager.GetParameterAsync(new GetParameterRequest()
    {
        Name = $"{builder.Configuration["SettingName"]}/{location}"
    }).Result.Parameter.Value);
}

builder.Services.AddSingleton<Settings>(new Settings(queueUrls));
builder.Services.AddHostedService<CheckActiveLocationsWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCorrelationId();

app.UseAuthorization();

app.MapControllers();

app.Run();