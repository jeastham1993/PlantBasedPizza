import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import { Network } from "./network";
import { EventBus } from "aws-cdk-lib/aws-events";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { HostedZone } from "aws-cdk-lib/aws-route53";
import { Certificate } from "aws-cdk-lib/aws-certificatemanager";
import { PrivateDnsNamespace } from "aws-cdk-lib/aws-servicediscovery";
import {
  CorsHttpMethod,
  DomainName,
  HttpApi,
  VpcLink,
} from "aws-cdk-lib/aws-apigatewayv2";
import { SubnetType } from "aws-cdk-lib/aws-ec2";

export class PlantBasedPizzaSharedInfrastructureStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const dnsName = process.env["DNS_NAME"];
    if (dnsName === undefined) {
      throw new Error("DNS_NAME is required");
    }
    const apiDnsName = process.env["API_DNS_NAME"];
    if (dnsName === undefined) {
      throw new Error("API_DNS_NAME is required");
    }
    const certArn = process.env["CERT_ARN"];
    if (dnsName === undefined) {
      throw new Error("CERT_ARN is required");
    }
    const hostedZoneId = process.env["HOSTED_ZONE_ID"]
    if (dnsName === undefined) {
      throw new Error("HOSTED_ZONE_ID is required");
    };
    const natInstanceId = process.env["NAT_INSTANCE_ID"]
    if (dnsName === undefined) {
      throw new Error("NAT_INSTANCE_ID is required");
    };

    const network = new Network(this, "PlantBasedPizzaNetworkResources", natInstanceId!);

    const hostedZone = HostedZone.fromHostedZoneAttributes(
      this,
      "HostedZone",
      {
        zoneName: dnsName,
        hostedZoneId: hostedZoneId!,
      }
    );

    const dnsNamespace = new PrivateDnsNamespace(this, "PrivateDnsNamespace", {
      name: "plantbasedpizza.local",
      vpc: network.vpc,
    });

    const namespace = PrivateDnsNamespace.fromPrivateDnsNamespaceAttributes(
      this,
      "Namespace",
      {
        namespaceName: "plantbasedpizza.local",
        namespaceArn: dnsNamespace.namespaceArn, // this is the ARN of the namespace
        namespaceId: dnsNamespace.namespaceId, // this is the ID of the namespace
      }
    );

    const domainName = new DomainName(this, "PlantBasedPizzaDomainName", {
      domainName: apiDnsName!,
      certificate: Certificate.fromCertificateArn(
        this,
        "PlantBasedPizzaCert",
        certArn!
      ),
    });

    const httpApi = new HttpApi(this, "PlantBasedPizzaHttpApi", {
      defaultDomainMapping: {
        domainName,
      },
      apiName: `PlantBasedPizzaApi-${id}`,
      corsPreflight: {
        allowOrigins: ["*"],
        allowMethods: [CorsHttpMethod.ANY],
        allowHeaders: ["*"],
      },
    });

    const internalHttpApi = new HttpApi(this, "PlantBasedPizzaInternalApi", {
      apiName: `PlantBasedPizzaInternalApi-${id}`,
    });

    const httpApiVpcLink = new VpcLink(this, "HttpApiVpcLink", {
      vpcLinkName: "PlantBasedPizzaVpcLink",
      vpc: network.vpc,
      subnets: network.vpc.selectSubnets({
        subnetType: SubnetType.PRIVATE_WITH_EGRESS,
      }),
      securityGroups: [network.noInboundAllOutboundSecurityGroup],
    });

    new StringParameter(this, "DnsNamespaceIdParam", {
      stringValue: namespace.namespaceId,
      parameterName: "/shared/namespace-id",
    });
    new StringParameter(this, "DnsNamespaceNameParam", {
      stringValue: namespace.namespaceName,
      parameterName: "/shared/namespace-name",
    });
    new StringParameter(this, "DnsNamespaceArnParam", {
      stringValue: namespace.namespaceArn,
      parameterName: "/shared/namespace-arn",
    });
    new StringParameter(this, "HttpApiId", {
      stringValue: httpApi.httpApiId,
      parameterName: "/shared/api-id",
    });
    new StringParameter(this, "InternalHttpApiId", {
      stringValue: internalHttpApi.httpApiId,
      parameterName: "/shared/internal-api-id",
    });
    new StringParameter(this, "VpcLinkId", {
      stringValue: httpApiVpcLink.vpcLinkId,
      parameterName: "/shared/vpc-link-id",
    });

    const eventBus = new EventBus(this, "PlantBasedPizzaEventBus", {
      eventBusName: "PlantBasedPizzaEvents",
    });

    const eventBusName = new StringParameter(this, "EventBusNameParam", {
      stringValue: eventBus.eventBusName,
      parameterName: "/shared/eb-name",
    });

    const ebArnParam = new StringParameter(this, "EventBusArnParam", {
      stringValue: eventBus.eventBusArn,
      parameterName: "/shared/eb-arn",
    });
  }
}
