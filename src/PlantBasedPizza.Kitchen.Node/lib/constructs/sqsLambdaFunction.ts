import { Construct } from "constructs";
import { NodejsFunction } from "aws-cdk-lib/aws-lambda-nodejs";
import { Runtime } from "aws-cdk-lib/aws-lambda";
import { Tags } from "aws-cdk-lib";
import { Alias } from "aws-cdk-lib/aws-kms";
import { SharedFunctionProps } from "./sharedFunctionProps";

export class InstrumentedSqsLambdaFunctionProps{
  sharedProps: SharedFunctionProps
  entry: string
  functionName: string
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
        ]
      }
    });

    props.sharedProps.databaseConnectionParam.grantRead(this.function);
    const kmsAlias = Alias.fromAliasName(this, "SSMAlias", "aws/ssm");
    kmsAlias.grantDecrypt(this.function);

    Tags.of(this.function).add('service', props.sharedProps.serviceName);
    Tags.of(this.function).add('env', props.sharedProps.environment);
    Tags.of(this.function).add('version', props.sharedProps.version);

    props.sharedProps.datadogConfiguration.addLambdaFunctions([this.function]);
  }
}
