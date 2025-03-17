import { Construct } from "constructs";
import { NodejsFunction, OutputFormat } from "aws-cdk-lib/aws-lambda-nodejs";
import { Code, Runtime } from "aws-cdk-lib/aws-lambda";
import { LambdaTarget } from "aws-cdk-lib/aws-elasticloadbalancingv2-targets";
import { ApplicationTargetGroup, ListenerCondition, TargetType } from "aws-cdk-lib/aws-elasticloadbalancingv2";
import { Tags } from "aws-cdk-lib";
import { Alias } from "aws-cdk-lib/aws-kms";
import { SharedProps } from "./sharedFunctionProps";
import { IStringParameter } from "aws-cdk-lib/aws-ssm";
import { HttpLambdaIntegration } from "aws-cdk-lib/aws-apigatewayv2-integrations";
import { HttpMethod } from "aws-cdk-lib/aws-events";
import { HttpRoute, HttpRouteKey } from "aws-cdk-lib/aws-apigatewayv2";

export class InstrumentedApiLambdaFunctionProps {
  sharedProps: SharedProps;
  handler: string;
  buildDef: string;
  outDir: string;
  path: string;
  method: string;
  priority: number;
  functionName: string;
  jwtKey: IStringParameter;
}

export class InstrumentedApiLambdaFunction extends Construct {
  function: NodejsFunction;

  constructor(scope: Construct, id: string, props: InstrumentedApiLambdaFunctionProps) {
    super(scope, id);

    const pathToBuildFile = props.buildDef;
    const pathToOutputFile = props.outDir;

    const code = Code.fromCustomCommand(pathToOutputFile, ["node", pathToBuildFile]);

    this.function = new NodejsFunction(this, props.functionName, {
      runtime: Runtime.NODEJS_20_X,
      code: code,
      handler: props.handler,
      memorySize: 512,
      environment: {
        TABLE_NAME: props.sharedProps.table.tableName,
        JWT_SSM_PARAM: props.jwtKey.parameterName,
        INTEGRATION_TEST_RUN: process.env.INTEGRATION_TEST ?? "",
      },
      bundling: {
        externalModules: ["graphql/language/visitor", "graphql/language/printer", "graphql/utilities"],
        esbuildVersion: "0.21.5",
      },
    });

    const kmsAlias = Alias.fromAliasName(this, "SSMAlias", "aws/ssm");
    kmsAlias.grantDecrypt(this.function);

    props.jwtKey.grantRead(this.function);

    const lambdaIntegration = new HttpLambdaIntegration(`LambdaIntegration${props.functionName}`, this.function);

    const route = new HttpRoute(this, `Route${props.functionName}`, {
      httpApi: props.sharedProps.apiProps.apiGateway,
      routeKey: HttpRouteKey.with(props.path, this.getHttpMethodFromString(props.method)),
      integration: lambdaIntegration
    });

    if (props.sharedProps.datadogConfiguration !== undefined) {
      props.sharedProps.datadogConfiguration.addLambdaFunctions([this.function]);
    }
  }

  getHttpMethodFromString(method: string): HttpMethod {
    switch (method.toUpperCase()) {
      case "GET":
        return HttpMethod.GET;
      case "POST":
        return HttpMethod.POST;
      case "PUT":
        return HttpMethod.PUT;
      case "DELETE":
        return HttpMethod.DELETE;
      case "HEAD":
        return HttpMethod.HEAD;
      case "OPTIONS":
        return HttpMethod.OPTIONS;
      case "PATCH":
        return HttpMethod.PATCH;
      default:
        throw `Unknown method ${method}`;
    }
  }
}
