using Datadog.Trace;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PlantBasedPizza_FunctionApp
{
    public class HttpExample
    {
        private readonly ILogger<HttpExample> _logger;

        public HttpExample(ILogger<HttpExample> logger)
        {
            _logger = logger;
        }

        [Function("HttpExample")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            using (var scope = Tracer.Instance.StartActive("http-example"))
            {
                _logger.LogInformation("C# HTTP trigger function processed a request.");
                return new OkObjectResult("Welcome to Azure Functions!");
            }
        }
    }
}
