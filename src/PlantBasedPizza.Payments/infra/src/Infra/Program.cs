﻿using System;
using Amazon;
using Amazon.CDK;
using Environment = Amazon.CDK.Environment;

namespace Infra
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new PaymentInfraStack(app, "PaymentServiceInfraStack", new StackProps()
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
