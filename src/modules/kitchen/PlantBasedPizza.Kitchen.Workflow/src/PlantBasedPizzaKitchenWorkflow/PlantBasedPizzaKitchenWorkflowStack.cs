using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Constructs;
using PlantBasedPizzaKitchenWorkflow.Constructs;

namespace PlantBasedPizzaKitchenWorkflow
{
    public class PlantBasedPizzaKitchenWorkflowStackSettings
    {
        public string Cell { get; set; } = "";
    }
    
    public class PlantBasedPizzaKitchenWorkflowStack : Stack
    {
        internal PlantBasedPizzaKitchenWorkflowStack(Construct scope, string id, PlantBasedPizzaKitchenWorkflowStackSettings settings, IStackProps props = null) : base(scope,
            id, props)
        {
            var serviceInfrastructure = new ServiceInfrastructure(this, $"BaseServiceInfrastructure{settings.Cell}", settings.Cell);

            var workflow = new KitchenWorkflowStack(this, $"Workflow-{settings.Cell}", settings.Cell);

            var newOrderHandlerFunction = new DotnetLambdaFunctionBuilder(this, new DotnetLambdaFunctionInitProps()
                {
                    FunctionName = $"NewOrderQueueHandler-{settings.Cell}",
                    FunctionHandler =
                        "PlantBasedPizza.Kitchen.Handlers::PlantBasedPizza.Kitchen.Handlers.Functions_QueueHandler_Generated::QueueHandler"
                })
                .AddEnvironmentVariable("TABLE_NAME", serviceInfrastructure.KitchenRequestTable.TableName)
                .AddEnvironmentVariable("KITCHEN_WORKFLOW", workflow.KitchenWorkflow.StateMachineArn)
                .AddQueueSource(serviceInfrastructure.InboundOrderStorageQueue)
                .Build();

            workflow.KitchenWorkflow.GrantStartExecution(newOrderHandlerFunction);
            serviceInfrastructure.KitchenRequestTable.GrantWriteData(newOrderHandlerFunction);
        }
    }
}