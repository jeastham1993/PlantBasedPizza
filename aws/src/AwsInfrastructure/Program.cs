using Amazon.CDK;
using AwsInfrastructure;

namespace Aws
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new LambdaStack(app, "LambdaStack", new StackProps { });
            app.Synth();
        }
    }
}
