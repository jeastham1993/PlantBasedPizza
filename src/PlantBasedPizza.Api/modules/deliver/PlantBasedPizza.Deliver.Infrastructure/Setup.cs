using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantBasedPizza.Deliver.Core.Commands;
using PlantBasedPizza.Deliver.Core.Configuration;
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
            // Register strongly typed configuration
            services.Configure<DeliveryOptions>(configuration.GetSection(DeliveryOptions.SectionName));

            // Register DbContext
            services.AddDbContext<DeliveryDbContext>(options =>
                options.UseNpgsql(
                    overrideConnectionString ?? configuration.GetConnectionString("DeliveryPostgresConnection"),
                    b => b.MigrationsAssembly("PlantBasedPizza.Deliver.Infrastructure")));

            services.AddScoped<IDeliveryRequestRepository, DeliveryRequestRepositoryPostgres>();
            services.AddTransient<Handles<OrderReadyForDeliveryEvent>, OrderReadyForDeliveryEventHandler>();
            services.AddTransient<GetDeliveryQueryHandler>();
            
            // Domain services and factories
            services.AddTransient<PlantBasedPizza.Deliver.Core.Services.IDeliveryRequestFactory, PlantBasedPizza.Deliver.Core.Services.DeliveryRequestFactory>();
            services.AddTransient<PlantBasedPizza.Deliver.Core.Services.IDeliveryDomainService, PlantBasedPizza.Deliver.Core.Services.DeliveryDomainService>();

            // Register validators
            services.AddScoped<IValidator<AssignDriverRequest>, AssignDriverRequestValidator>();
            services.AddScoped<IValidator<MarkOrderDeliveredRequest>, MarkOrderDeliveredRequestValidator>();

            return services;
        }
    }
}