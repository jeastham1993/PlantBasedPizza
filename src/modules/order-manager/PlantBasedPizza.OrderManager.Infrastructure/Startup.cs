using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure
{
    public static class Startup
    {
        public static IServiceProvider? Services;

        public static void Configure()
        {
            var collection = new ServiceCollection();

            collection.AddTransient<IOrderRepository, OrderRepository>();
            collection.AddTransient<IRecipeService, RecipeService>();

            Shared.Setup.AddSharedInfrastructure(collection);

            Services = collection.BuildServiceProvider();
        }
    }
}
