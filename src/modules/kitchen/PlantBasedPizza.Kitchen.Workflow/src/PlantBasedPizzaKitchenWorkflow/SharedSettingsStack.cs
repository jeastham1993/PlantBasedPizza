using Amazon.CDK;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace PlantBasedPizzaKitchenWorkflow
{
    public class SharedSettingsStack : Stack
    {
        internal SharedSettingsStack(Construct scope, string id, string[] activeLocations, IStackProps props = null) : base(scope,
            id, props)
        {
            var secret = new StringParameter(this, $"AvailableLocationSetting", new StringParameterProps()
            {
                ParameterName = "/plant-based-pizza/kitchen/active-locations",
                Description = "Current active locations",
                StringValue = string.Join(',', activeLocations)
            });
        }
    }
}