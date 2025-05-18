using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Core.GetDelivery;
using PlantBasedPizza.Deliver.Core.Handlers;
using PlantBasedPizza.Events;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Deliver.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddDeliveryModuleInfrastructure(this IServiceCollection services,
            IConfiguration configuration, string? overrideConnectionString = null)
        {
            // Register DbContext
            services.AddDbContext<DeliveryDbContext>(options =>
                options.UseNpgsql(
                    overrideConnectionString ?? configuration.GetConnectionString("DeliveryPostgresConnection"),
                    b => b.MigrationsAssembly("PlantBasedPizza.Deliver.Infrastructure")));

            services.AddScoped<IDeliveryRequestRepository, DeliveryRequestRepositoryPostgres>();
            services.AddTransient<Handles<OrderReadyForDeliveryEvent>, OrderReadyForDeliveryEventHandler>();
            services.AddTransient<GetDeliveryQueryHandler>();

            return services;
        }
    }
}