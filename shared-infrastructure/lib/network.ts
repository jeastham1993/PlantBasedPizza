import { CfnOutput } from "aws-cdk-lib";
import { CfnRoute, GatewayVpcEndpointAwsService, IVpc, SecurityGroup, SubnetType, Vpc } from "aws-cdk-lib/aws-ec2";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { Construct } from "constructs";

export class Network extends Construct {

    vpc: IVpc;

    constructor(scope: Construct, id: string) {
        super(scope, id);

        const natInstanceId = process.env['NAT_INSTANCE_ID']!;

        this.vpc = new Vpc(this, "PlantBasedPizzaNetwork", {
            natGateways: 0,
            vpcName: 'PlantBasedPizza',
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

          this.vpc.privateSubnets.forEach(({ routeTable: { routeTableId } }, index) => {
            new CfnRoute(this, 'PrivateSubnetOutboundToNatInstance' + index, {
              destinationCidrBlock: '0.0.0.0/0',
              routeTableId,
              instanceId: natInstanceId
            })
            
          })
      
          const noInboundAllOutboundSecurityGroup = new SecurityGroup(this, "noInboundAllOutboundSecurityGroup", {
            vpc: this.vpc,
            allowAllOutbound: true,
            description: "No inbound / all outbound",
            securityGroupName: "noInboundAllOutboundSecurityGroup",
          })

          const vpcIdParameter = new StringParameter(this, "VPCIdParameter", {
            stringValue: this.vpc.vpcId,
            parameterName: '/shared/vpc-id'
          });

          new CfnOutput(this, "noInboundAllOutboundSecurityGroupOutput", {
            exportName: "noInboundAllOutboundSecurityGroup",
            value: noInboundAllOutboundSecurityGroup.securityGroupId,
          })
    }
}