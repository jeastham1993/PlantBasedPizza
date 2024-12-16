using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;

var armClient = new ArmClient(new DefaultAzureCredential());
var subscription = await armClient.GetDefaultSubscriptionAsync();
var resourceGroups = subscription.GetResourceGroups();
var resourceGroup = await resourceGroups.GetAsync("plant-based-pizza-payments-test");
var paymentContainerApp = resourceGroup.Value.GetContainerApps();

var runMode = Environment.GetEnvironmentVariable("RUN_MODE") ?? "stop";

await foreach (var app in paymentContainerApp.GetAllAsync())
{
    var latestRevision = await app.GetContainerAppRevisionAsync(app.Data.LatestRevisionName);

    Console.WriteLine($"Latest revision running state is: {latestRevision.Value.Data.RunningState.Value}");

    if (runMode.ToLower() == "start")
    {
        Console.WriteLine($"Starting {app.Data.Name}...");
        
        await app.StartAsync(WaitUntil.Completed);
        
        var runningState = RevisionRunningState.Unknown.ToString();

        while (runningState != RevisionRunningState.Running && runningState != "RunningAtMaxScale")
        {
            latestRevision = await app.GetContainerAppRevisionAsync(app.Data.LatestRevisionName);

            runningState = (latestRevision.Value.Data.RunningState ?? RevisionRunningState.Unknown).ToString();

            Console.WriteLine($"Running state is: {runningState}");

            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
    else
    {
        Console.WriteLine($"Stopping {app.Data.Name}...");
        
        await app.StopAsync(WaitUntil.Completed);
    }
}