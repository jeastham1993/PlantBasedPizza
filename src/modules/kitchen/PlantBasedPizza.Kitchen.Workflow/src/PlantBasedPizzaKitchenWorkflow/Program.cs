using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlantBasedPizzaKitchenWorkflow
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            var activeLocations = new string[2] {"KITCH001", "KITCH002"};

            foreach (var location in activeLocations)
            {
                new PlantBasedPizzaKitchenWorkflowStack(app, $"PlantBasedPizzaKitchenWorkflowStack{location}", new PlantBasedPizzaKitchenWorkflowStackSettings()
                {
                    Cell = location
                }, new StackProps()
                {
                    StackName = $"PlantBasedPizzaKitchenWorkflowStack-{location}",
                });   
            }

            var sharedSettings = new SharedSettingsStack(app, "SharedSettings", activeLocations, new StackProps(){});

            app.Synth();
        }
    }
}
