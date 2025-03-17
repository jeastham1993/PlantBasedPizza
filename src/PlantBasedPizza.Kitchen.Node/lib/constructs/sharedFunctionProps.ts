import { HttpApi, IHttpApi } from "aws-cdk-lib/aws-apigatewayv2";
import { ITable } from "aws-cdk-lib/aws-dynamodb";
import { IApplicationListener } from "aws-cdk-lib/aws-elasticloadbalancingv2";
import { DatadogLambda } from "datadog-cdk-constructs-v2";

export interface SharedProps {
  serviceName: string;
  environment: string;
  version: string;
  datadogConfiguration: DatadogLambda | undefined;
  apiProps: ApiProps;
  table: ITable
}

export interface ApiProps {
  apiGateway: IHttpApi
}
