using PlantBasedPizza.Deliver.Infrastructure;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Recipes.Infrastructure;
using PlantBasedPizza.Kitchen.Infrastructure;
using PlantBasedPizza.Shared;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Logging;

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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
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