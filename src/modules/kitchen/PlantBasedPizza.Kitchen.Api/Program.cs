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
var queueUrl = systemsManager.GetParameterAsync(new GetParameterRequest()
{
    Name = builder.Configuration["SettingName"]
}).Result.Parameter.Value;

builder.Services.AddSingleton<Settings>(new Settings(queueUrl));

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