using Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Deliver.Core.OrderReadyForDelivery;
using PlantBasedPizza.Events;

namespace PlantBasedPizza.Delivery.Worker;

public static class Setup
{
    public static WebApplication AddReadyForDeliveryHandler(this WebApplication app)
    {
        

        return app;
    }
}