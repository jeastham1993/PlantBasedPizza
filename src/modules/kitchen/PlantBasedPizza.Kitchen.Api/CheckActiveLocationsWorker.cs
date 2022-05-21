using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace PlantBasedPizza.Kitchen.Api;

public class CheckActiveLocationsWorker : BackgroundService
{
    private readonly Settings _settings;
    private readonly AmazonSimpleSystemsManagementClient _systemsManagementClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CheckActiveLocationsWorker> _logger;

    public CheckActiveLocationsWorker(Settings settings, AmazonSimpleSystemsManagementClient systemsManagementClient, IConfiguration configuration, ILogger<CheckActiveLocationsWorker> logger)
    {
        _settings = settings;
        _systemsManagementClient = systemsManagementClient;
        _configuration = configuration;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var activeKitchens = _systemsManagementClient.GetParameterAsync(new GetParameterRequest()
            {
                Name = this._configuration["ActiveLocationsParameterName"]
            }).Result.Parameter.Value;

            var queueUrls = new Dictionary<string, string>();

            foreach (var location in activeKitchens.Split(','))
            {
                queueUrls.Add(location, this._systemsManagementClient.GetParameterAsync(new GetParameterRequest()
                {
                    Name = $"{this._configuration["SettingName"]}/{location}"
                }).Result.Parameter.Value);
            }

            this._settings.QueueUrls = queueUrls;

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}