using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.OrderManager.Core.Command;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.OrderManager.Core.Services;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.OrderManager.Infrastructure.Controllers
{
    public class HealthController : ControllerBase 
    {
        private readonly IObservabilityService _logger;
        private readonly AmazonDynamoDBClient _dynamoDbClient;

        public HealthController(IObservabilityService logger, AmazonDynamoDBClient dynamoDbClient)
        {
            _logger = logger;
            _dynamoDbClient = dynamoDbClient;
        }

        [HttpGet("health")]
        public async Task<IActionResult> Get(string orderIdentifier)
        {
            var tableDetails = await this._dynamoDbClient.DescribeTableAsync(InfrastructureConstants.TableName);

            return this.Ok();
        }
    }
}