using System.Text.Json;
using Azure.Messaging.ServiceBus;
using PlantBasedPizza.Recipes.Core.OrderCompletedHandler;

namespace PlantBasedPizza.Recipes.Api;

public class BackgroundWorker : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<BackgroundWorker> _logger;
    private readonly OrderCompletedHandler _orderCompletedHandler;

    public BackgroundWorker(
        ServiceBusClient serviceBusClient,
        IConfiguration configuration,
        ILogger<BackgroundWorker> logger,
        OrderCompletedHandler orderCompletedHandler)
    {
        _logger = logger;
        _orderCompletedHandler = orderCompletedHandler;

        // Get the queue name from configuration
        string queueName = configuration["AZURE_SERVICE_BUS_QUEUE_NAME"];
        
        // Create the processor
        _processor = serviceBusClient.CreateProcessor(queueName);
        
        // Set up handlers
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start processing
        await _processor.StartProcessingAsync(stoppingToken);
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
        finally
        {
            // Stop the processor
            await _processor.StopProcessingAsync(stoppingToken);
        }
    }
    
    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var messageBody = args.Message.Body.ToString();
        _logger.LogInformation("Received message: {body}", messageBody);
        
        // Process your message here
        await this._orderCompletedHandler.Handle(JsonSerializer.Deserialize<OrderCompletedEventV2>(messageBody));
        
        // Complete the message
        await args.CompleteMessageAsync(args.Message, CancellationToken.None);
    }
    
    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error processing message: {source}", args.ErrorSource);
        return Task.CompletedTask;
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}