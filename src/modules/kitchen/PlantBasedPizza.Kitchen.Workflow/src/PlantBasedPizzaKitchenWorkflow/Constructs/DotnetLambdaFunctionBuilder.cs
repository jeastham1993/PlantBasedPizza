using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace PlantBasedPizzaKitchenWorkflow.Constructs
{
    public class DotnetLambdaFunctionInitProps
    {
        public string FunctionName { get; set; }
        public string FunctionHandler { get; set; }
    }
    public class DotnetLambdaFunctionBuilder
    {
        private Function _function;
        private readonly Construct _scope;
        private readonly string _handler;
        private readonly string _name;
        private Dictionary<string, string> _environmentVariables;
        private List<IEventSource> _sources;

        public DotnetLambdaFunctionBuilder(Construct scope, DotnetLambdaFunctionInitProps props)
        {
            if (string.IsNullOrEmpty(props.FunctionHandler))
                throw new ArgumentNullException();

            this._scope = scope;
            this._handler = props.FunctionHandler;
            this._name = props.FunctionName;
            this._sources = new List<IEventSource>();
        }

        public DotnetLambdaFunctionBuilder AddEnvironmentVariable(string name, string value)
        {
            if (this._environmentVariables == null)
            {
                this._environmentVariables = new Dictionary<string, string>();
            }
            
            this._environmentVariables.Add(name, value);

            return this;
        }

        public DotnetLambdaFunctionBuilder AddQueueSource(Queue queue)
        {
            this._sources.Add(new SqsEventSource(queue, new SqsEventSourceProps()
            {
                Enabled = true,
            }));

            return this;
        }
        
        public Function Build()
        {
            this._function = new Function(this._scope,
                this._name,
                new FunctionProps
                {
                    Runtime = Runtime.DOTNET_6,
                    Code = Code.FromAsset("./src/output.zip"),
                    Handler = this._handler,
                    Environment = this._environmentVariables,
                    Tracing = Tracing.ACTIVE,
                    LogRetention = RetentionDays.ONE_DAY,
                    Timeout = Duration.Seconds(30)
                });

            foreach (var source in this._sources)
            {
                this._function.AddEventSource(source);
            }

            return this._function;
        }
    }
}