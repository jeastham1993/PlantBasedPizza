using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlantBasedPizza.Events;

public class RetrieveEventConsumerResponse
{
    public RetrieveEventConsumerResponse(IModel channel)
    {
        Channel = channel;
        Consumer = new EventingBasicConsumer(channel);
    }
    
    public EventingBasicConsumer Consumer { get; private set; }

    public IModel Channel { get; private set; }
}