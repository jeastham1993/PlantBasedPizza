import { IVpc } from "aws-cdk-lib/aws-ec2";
import { IApplicationListener } from "aws-cdk-lib/aws-elasticloadbalancingv2";
import { IEventBus } from "aws-cdk-lib/aws-events";
import { IStringParameter } from "aws-cdk-lib/aws-ssm";
import { Datadog } from "datadog-cdk-constructs-v2";

export interface SharedFunctionProps {
  serviceName: string;
  environment: string;
  version: string;
  datadogConfiguration: Datadog;
  albListener: IApplicationListener;
  vpc: IVpc;
  databaseConnectionParam: IStringParameter;
}
