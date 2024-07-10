import { HttpApi } from "aws-cdk-lib/aws-apigatewayv2";
import { ITable } from "aws-cdk-lib/aws-dynamodb";
import { IApplicationListener } from "aws-cdk-lib/aws-elasticloadbalancingv2";
import { Datadog } from "datadog-cdk-constructs-v2";

export interface SharedProps {
  serviceName: string;
  environment: string;
  version: string;
  datadogConfiguration: Datadog | undefined;
  apiProps: ApiProps;
  table: ITable
}

export interface ApiProps {
  albListener: IApplicationListener | undefined;
  apiGateway: HttpApi | undefined
}
