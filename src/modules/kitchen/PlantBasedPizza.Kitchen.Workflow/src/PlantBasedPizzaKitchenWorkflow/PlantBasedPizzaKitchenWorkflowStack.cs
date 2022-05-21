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
    public class PlantBasedPizzaKitchenWorkflowStack : Stack
    {
        internal PlantBasedPizzaKitchenWorkflowStack(Construct scope, string id, IStackProps props = null) : base(scope,
            id, props)
        {
            var serviceInfrastructure = new ServiceInfrastructure(this, "BaseServiceInfrastructure");

            var newOrderHandlerFunction = new DotnetLambdaFunctionBuilder(this, new DotnetLambdaFunctionInitProps()
                {
                    FunctionName = "NewOrderQueueHandler",
                    FunctionHandler =
                        "PlantBasedPizza.Kitchen.Handlers::PlantBasedPizza.Kitchen.Handlers.Functions_QueueHandler_Generated::QueueHandler"
                })
                .AddEnvironmentVariable("TABLE_NAME", serviceInfrastructure.KitchenRequestTable.TableName)
                .AddQueueSource(serviceInfrastructure.InboundOrderStorageQueue)
                .Build();
            
            serviceInfrastructure.KitchenRequestTable.GrantWriteData(newOrderHandlerFunction);
        }
    }
}