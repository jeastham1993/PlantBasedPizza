using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Api.Services;

namespace PlantBasedPizza.Kitchen.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<KitchenController> _logger;
    private readonly IQueueManager _queueManager;

    public HealthController(ILogger<KitchenController> logger, IQueueManager queueManager)
    {
        _logger = logger;
        _queueManager = queueManager;
    }

    [HttpGet]
    public async Task<IActionResult> CheckHealth()
    {
        await this._queueManager.CheckQueueStatus();
        
        return new OkResult();
    }
}