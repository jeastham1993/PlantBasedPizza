import { Construct } from "constructs";
import { NodejsFunction, OutputFormat } from "aws-cdk-lib/aws-lambda-nodejs";
import { Code, Runtime } from "aws-cdk-lib/aws-lambda";
import { LambdaTarget } from "aws-cdk-lib/aws-elasticloadbalancingv2-targets";
import {
  ApplicationTargetGroup,
  ListenerCondition,
  TargetType,
} from "aws-cdk-lib/aws-elasticloadbalancingv2";
import { Tags } from "aws-cdk-lib";
import { Alias } from "aws-cdk-lib/aws-kms";
import { SharedProps } from "./sharedFunctionProps";
import { IStringParameter } from "aws-cdk-lib/aws-ssm";

export class InstrumentedApiLambdaFunctionProps {
  sharedProps: SharedProps;
  handler: string;
  buildDef: string;
  outDir: string;
  path: string;
  methods: string[];
  priority: number;
  functionName: string;
  jwtKey: IStringParameter
}

export class InstrumentedApiLambdaFunction extends Construct {
  function: NodejsFunction;

  constructor(scope: Construct, id: string, props: InstrumentedApiLambdaFunctionProps) {
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
      environment: {
        CONN_STRING_PARAM: props.sharedProps.databaseConnectionParam.parameterName,
        TABLE_NAME: props.sharedProps.table.tableName,
        JWT_SSM_PARAM: props.jwtKey.parameterName,
        DD_IAST_ENABLED: "true"
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

    props.sharedProps.databaseConnectionParam.grantRead(this.function);
    const kmsAlias = Alias.fromAliasName(this, "SSMAlias", "aws/ssm");
    kmsAlias.grantDecrypt(this.function);

    Tags.of(this.function).add("service", props.sharedProps.serviceName);
    Tags.of(this.function).add("env", props.sharedProps.environment);
    Tags.of(this.function).add("version", props.sharedProps.version);

    props.sharedProps.datadogConfiguration.addLambdaFunctions([this.function]);
    props.jwtKey.grantRead(this.function);

    const getNewLambdaTarget = new LambdaTarget(this.function);

    const targetGroup = new ApplicationTargetGroup(this, `${id}TargetGroup`, {
      targetType: TargetType.LAMBDA,
    });
    targetGroup.addTarget(getNewLambdaTarget);

    props.sharedProps.albListener.addTargetGroups(`${id}LambdaTargetGroup`, {
      targetGroups: [targetGroup],
      conditions: [ListenerCondition.pathPatterns([props.path]), ListenerCondition.httpRequestMethods(props.methods)],
      priority: props.priority,
    });
  }
}
