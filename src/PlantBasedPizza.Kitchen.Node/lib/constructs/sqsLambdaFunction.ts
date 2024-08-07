import { Construct } from "constructs";
import { NodejsFunction } from "aws-cdk-lib/aws-lambda-nodejs";
import { Code, Runtime } from "aws-cdk-lib/aws-lambda";
import { Duration, Tags } from "aws-cdk-lib";
import { Alias } from "aws-cdk-lib/aws-kms";
import { SharedProps } from "./sharedFunctionProps";
import { IQueue } from "aws-cdk-lib/aws-sqs";
import { SqsEventSource } from "aws-cdk-lib/aws-lambda-event-sources";

export class InstrumentedSqsLambdaFunctionProps{
  sharedProps: SharedProps
  handler: string;
  buildDef: string;
  outDir: string;
  functionName: string
  queue: IQueue
};

export class InstrumentedSqsLambdaFunction extends Construct {
  function: NodejsFunction;

  constructor(scope: Construct, id: string, props: InstrumentedSqsLambdaFunctionProps) {
    super(scope, id);

    const pathToBuildFile = props.buildDef;
    const pathToOutputFile = props.outDir;
    
    const code = Code.fromCustomCommand(
      pathToOutputFile,
      ['node', pathToBuildFile],
    );

    this.function = new NodejsFunction(this, props.functionName, {
      runtime: Runtime.NODEJS_20_X,
      code: code,
      handler: props.handler,
      memorySize: 512,
      timeout: Duration.seconds(20),
      environment: {
        RECIPE_API_ENDPOINT: 'https://api.dev.plantbasedpizza.net',
        TABLE_NAME: props.sharedProps.table.tableName,
        INTEGRATION_TEST_RUN: process.env.INTEGRATION_TEST ?? ""
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

    const kmsAlias = Alias.fromAliasName(this, "SSMAlias", "aws/ssm");
    kmsAlias.grantDecrypt(this.function);

    if (props.sharedProps.datadogConfiguration !== undefined){
      props.sharedProps.datadogConfiguration.addLambdaFunctions([this.function]);
    }
  }
}
