using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;
using AwsInfrastructure;

namespace Aws
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new AwsStack(app, "AwsStack", new StackProps { });
            app.Synth();
        }
    }
}
