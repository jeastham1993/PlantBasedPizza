import { CfnOutput } from "aws-cdk-lib";
import {
  CfnRoute,
  GatewayVpcEndpointAwsService,
  IVpc,
  Peer,
  Port,
  SecurityGroup,
  SubnetType,
  Vpc,
} from "aws-cdk-lib/aws-ec2";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { Construct } from "constructs";

export class Network extends Construct {
  vpc: IVpc;
  noInboundAllOutboundSecurityGroup: SecurityGroup;

  constructor(scope: Construct, id: string) {
    super(scope, id);

    this.vpc = new Vpc(this, "PlantBasedPizzaNetwork", {
      natGateways: 2,
      vpcName: "PlantBasedPizza",
      subnetConfiguration: [
        {
          name: "public-subnet",
          subnetType: SubnetType.PUBLIC,
          cidrMask: 24,
        },
        {
          name: "private-subnet",
          subnetType: SubnetType.PRIVATE_WITH_EGRESS,
          cidrMask: 24,
        },
      ],
    });

    this.vpc.addGatewayEndpoint("dynamoDBEndpoint", {
      service: GatewayVpcEndpointAwsService.DYNAMODB,
    });
    this.vpc.addGatewayEndpoint("s3Endpoint", {
      service: GatewayVpcEndpointAwsService.S3,
    });

    this.noInboundAllOutboundSecurityGroup = new SecurityGroup(
      this,
      "noInboundAllOutboundSecurityGroup",
      {
        vpc: this.vpc,
        allowAllOutbound: true,
        description: "No inbound / all outbound",
        securityGroupName: "noInboundAllOutboundSecurityGroup",
      }
    );
    this.noInboundAllOutboundSecurityGroup.addIngressRule(
      this.noInboundAllOutboundSecurityGroup,
      Port.tcp(8080),
      "allow self"
    );
    this.noInboundAllOutboundSecurityGroup.addIngressRule(
      Peer.ipv4(this.vpc.vpcCidrBlock),
      Port.tcp(8080)
    );

    const vpcIdParameter = new StringParameter(this, "VPCIdParameter", {
      stringValue: this.vpc.vpcId,
      parameterName: "/shared/vpc-id",
    });

    const vpcLinkSgParameter = new StringParameter(this, "VPCLinkSecurityGroupParameter", {
      stringValue: this.noInboundAllOutboundSecurityGroup.securityGroupId,
      parameterName: "/shared/vpc-link-sg-id",
    });
  }
}
