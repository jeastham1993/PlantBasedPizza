using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PlantBasedPizza.Deliver.Core.Entities;
using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;
using Saunter;
using Saunter.AsyncApiSchema.v2;

namespace PlantBasedPizza.Api
{
    /// <summary>
    /// Encapsulates logic for application startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The applications <see cref="IConfiguration"/>.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The application <see cref="IServiceCollection"/>.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
            services.AddOrderManagerInfrastructure(this.Configuration);
            services.AddRecipeInfrastructure(this.Configuration);
            services.AddKitchenInfrastructure(this.Configuration);
            services.AddDeliveryModuleInfrastructure(this.Configuration);
            services.AddSharedInfrastructure(this.Configuration);
            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Plant Based Pizza API",
                    Description = "The API for the Plant Based Pizza API",
                    Contact = new OpenApiContact
                    {
                        Name = "James Eastham",
                        Url = new Uri("https://jameseastham.co.uk")
                    },
                });

                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PlantBasedPizza.Deliver.Infrastructure.xml"));
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PlantBasedPizza.Kitchen.Infrastructure.xml"));
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PlantBasedPizza.OrderManager.Infrastructure.xml"));
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PlantBasedPizza.Recipes.Infrastructure.xml"));
            });

            services.AddAsyncApiSchemaGeneration(options =>
            {
                // Specify example type(s) from assemblies to scan.
                options.AssemblyMarkerTypes = new[] { typeof(DriverCollectedOrderEvent), typeof(Order), typeof(DeliveryRequest), typeof(KitchenRequest) };

                // Build as much (or as little) of the AsyncApi document as you like.
                // Saunter will generate Channels, Operations, Messages, etc, but you
                // may want to specify Info here.
                options.AsyncApi = new AsyncApiDocument
                {
                    Info = new Info("Plant Based Pizza API", "1.0.0")
                    {
                        Description = "The Plant Based Pizza event messages..",
                    },
                };
            });

            services.AddControllers();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">A <see cref="IApplicationBuilder"/>.</param>
        /// <param name="env">A <see cref="IWebHostEnvironment"/>.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseXRay("PlantBasedPizza.Api");

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAsyncApiDocuments();
                endpoints.MapAsyncApiUi();
                endpoints.MapControllers();
            });

            DomainEvents.Container = app.ApplicationServices;

            app.Use(async (context, next) =>
            {
                var observability = app.ApplicationServices.GetService<IObservabilityService>();

                var correlationId = string.Empty;

                if (context.Request.Headers.ContainsKey("X-Amzn-Trace-Id"))
                {
                    correlationId = context.Request.Headers["X-Amzn-Trace-Id"].ToString();

                    context.Request.Headers.Add(CorrelationContext.DefaultRequestHeaderName, correlationId);
                }
                else if (context.Request.Headers.ContainsKey(CorrelationContext.DefaultRequestHeaderName))
                {
                    correlationId = context.Request.Headers[CorrelationContext.DefaultRequestHeaderName].ToString();
                }
                else
                {
                    correlationId = Guid.NewGuid().ToString();

                    context.Request.Headers.Add(CorrelationContext.DefaultRequestHeaderName, correlationId);
                }

                var timer = new System.Timers.Timer();

                timer.Start();

                CorrelationContext.SetCorrelationId(correlationId);

                observability.Info($"Request received to {context.Request.Path.Value}");

                context.Response.Headers.Add(CorrelationContext.DefaultRequestHeaderName, correlationId);

                // Do work that doesn't write to the Response.
                await next.Invoke();

                timer.Stop();

                var routesToIgnore = new string[3]
                {
        "health",
        "faivcon.ico",
        "swagger"
                };

                var pathRoute = context.Request.Path.Value.Split('/');

                if (routesToIgnore.Contains(pathRoute[1]) == false)
                {
                    observability.PutMetric(pathRoute[1], $"{pathRoute[^1]}-Latency", timer.Interval).Wait();
                }
            });
        }
    }
}