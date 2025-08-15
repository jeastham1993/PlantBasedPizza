using Amazon.CDK;

namespace AwsInfrastructure
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
