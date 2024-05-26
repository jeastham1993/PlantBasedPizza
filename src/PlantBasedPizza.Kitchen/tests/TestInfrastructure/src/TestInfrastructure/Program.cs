using Amazon.CDK;
using Environment = Amazon.CDK.Environment;

namespace TestInfrastructure
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var version = System.Environment.GetEnvironmentVariable("BUILD_VERSION");
            
            var app = new App();
            new KitchenServiceTestInfrastructure(app, $"KitchenServiceTestInfrastructure-{version}", new ApplicationStackProps()
            {
                Version = version
            }, 
                new StackProps()
            {
                Env = new Environment()
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
                }
            });
            app.Synth();
        }
    }
}
