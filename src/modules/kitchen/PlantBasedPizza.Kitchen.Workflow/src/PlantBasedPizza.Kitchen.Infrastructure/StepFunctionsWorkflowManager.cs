using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Newtonsoft.Json;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Services;

namespace PlantBasedPizza.Kitchen.Infrastructure;

public class StepFunctionsWorkflowManager : IWorkflowManager
{
    private readonly AmazonStepFunctionsClient _stepFunctionsClient;

    public StepFunctionsWorkflowManager(AmazonStepFunctionsClient stepFunctionsClient)
    {
        _stepFunctionsClient = stepFunctionsClient;
    }

    public async Task StartWorkflowExecution(KitchenRequest request)
    {
        await this._stepFunctionsClient.StartExecutionAsync(new StartExecutionRequest()
        {
            Input = JsonConvert.SerializeObject(request),
            Name = $"{request.OrderIdentifier}-{Guid.NewGuid()}",
            StateMachineArn = Environment.GetEnvironmentVariable("KITCHEN_WORKFLOW")
        });
    }
}