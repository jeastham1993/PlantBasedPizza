import { Construct } from "constructs";
import { NodejsFunction } from "aws-cdk-lib/aws-lambda-nodejs";
import { Runtime } from "aws-cdk-lib/aws-lambda";
import { Duration, Tags } from "aws-cdk-lib";
import { Alias } from "aws-cdk-lib/aws-kms";
import { SharedProps } from "./sharedFunctionProps";
import { IQueue } from "aws-cdk-lib/aws-sqs";
import { SqsEventSource } from "aws-cdk-lib/aws-lambda-event-sources";

export class InstrumentedSqsLambdaFunctionProps{
  sharedProps: SharedProps
  entry: string
  functionName: string
  queue: IQueue
};

export class InstrumentedSqsLambdaFunction extends Construct {
  function: NodejsFunction;

  constructor(scope: Construct, id: string, props: InstrumentedSqsLambdaFunctionProps) {
    super(scope, id);
    this.function = new NodejsFunction(this, props.functionName, {
      runtime: Runtime.NODEJS_20_X,
      projectRoot: './',
      entry: props.entry,
      depsLockFilePath: './package-lock.json',
      memorySize: 512,
      timeout: Duration.seconds(20),
      environment: {
        CONN_STRING_PARAM: props.sharedProps.databaseConnectionParam.parameterName,
        RECIPE_API_ENDPOINT: 'https://api.dev.plantbasedpizza.net',
        TABLE_NAME: props.sharedProps.table.tableName
      },
      bundling: {
        externalModules: [
          "graphql/language/visitor",
          "graphql/language/printer",
          "graphql/utilities"
        ],
        esbuildVersion: "0.21.5"
      }
    });

    this.function.addEventSource(new SqsEventSource(props.queue));

    props.sharedProps.databaseConnectionParam.grantRead(this.function);
    const kmsAlias = Alias.fromAliasName(this, "SSMAlias", "aws/ssm");
    kmsAlias.grantDecrypt(this.function);

    Tags.of(this.function).add('service', props.sharedProps.serviceName);
    Tags.of(this.function).add('env', props.sharedProps.environment);
    Tags.of(this.function).add('version', props.sharedProps.version);

    props.sharedProps.datadogConfiguration.addLambdaFunctions([this.function]);
  }
}
