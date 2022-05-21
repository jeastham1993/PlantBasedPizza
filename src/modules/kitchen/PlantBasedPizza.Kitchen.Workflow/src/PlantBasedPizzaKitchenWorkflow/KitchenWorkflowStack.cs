using Amazon.CDK.AWS.StepFunctions;
using Constructs;

namespace PlantBasedPizzaKitchenWorkflow
{
    public class KitchenWorkflowStack : Construct
    {
        public StateMachine KitchenWorkflow { get; private set; }
        
        public KitchenWorkflowStack(Construct scope, string id, string cell) : base(scope, id)
        {
            var kitchenWorkflow = new StateMachine(this, $"KitchenWorkflow-{cell}", new StateMachineProps()
            {
                Definition = new Pass(this, $"Workflow{cell}-Pass", new PassProps()
                {
                    
                })
            });

            this.KitchenWorkflow = kitchenWorkflow;
        }
    }
}