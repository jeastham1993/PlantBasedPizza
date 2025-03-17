import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import { Network } from "./network";
import { EventBus } from "aws-cdk-lib/aws-events";
import {
  ApplicationListener,
  ApplicationLoadBalancer,
  ListenerAction,
} from "aws-cdk-lib/aws-elasticloadbalancingv2";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { ARecord, HostedZone, RecordTarget } from "aws-cdk-lib/aws-route53";
import { Certificate } from "aws-cdk-lib/aws-certificatemanager";
import {
  LoadBalancerTarget,
  CloudFrontTarget,
} from "aws-cdk-lib/aws-route53-targets";
import { HttpOrigin } from "aws-cdk-lib/aws-cloudfront-origins";
import {
  AllowedMethods,
  CachePolicy,
  Distribution,
  ResponseHeadersPolicy,
  ViewerProtocolPolicy,
} from "aws-cdk-lib/aws-cloudfront";
import { PrivateDnsNamespace } from "aws-cdk-lib/aws-servicediscovery";
import { HttpApi, VpcLink } from "aws-cdk-lib/aws-apigatewayv2";
import { SubnetType } from "aws-cdk-lib/aws-ec2";

export class PlantBasedPizzaSharedInfrastructureStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const network = new Network(this, "PlantBasedPizzaNetworkResources");
    const cloudfrontDnsName = process.env["CLOUDFRONT_DNS_NAME"]!;
    const dnsName = process.env["DNS_NAME"]!;
    const apiDnsName = process.env["API_DNS_NAME"]!;
    const internalDnsName = process.env["INTERNAL_API_DNS_NAME"]!;
    const certArn = process.env["CERT_ARN"]!;
    const usEastCertArn = process.env["US_EAST_1_CERT_ARN"]!;
    const hostedZoneId = process.env["HOSTED_ZONE_ID"]!;

    // const hostedZoned = HostedZone.fromHostedZoneAttributes(
    //   this,
    //   "HostedZone",
    //   {
    //     zoneName: dnsName,
    //     hostedZoneId: hostedZoneId,
    //   }
    // );

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

    const httpApi = new HttpApi(this, "PlantBasedPizzaHttpApi", {
      apiName: `PlantBasedPizzaApi-${id}`,
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

    // const certificate = Certificate.fromCertificateArn(
    //   this,
    //   "PlantBasedPizzaCert",
    //   certArn
    // );
    // const usEast1Cert = Certificate.fromCertificateArn(
    //   this,
    //   "UsEast1Cert",
    //   usEastCertArn
    // );

    // const sharedAlbWithListener = new ApplicationLoadBalancer(
    //   this,
    //   "ApplicationIngressWithListener",
    //   {
    //     loadBalancerName: "plant-based-pizza-ingress",
    //     vpc: network.vpc,
    //     internetFacing: true,
    //   }
    // );
    // const internalAlbWithListener = new ApplicationLoadBalancer(
    //   this,
    //   "InternalIngressWithListener",
    //   {
    //     loadBalancerName: "plant-based-pizza-internal",
    //     vpc: network.vpc,
    //     internetFacing: false,
    //   }
    // );

    // new ARecord(this, "DnsRecord", {
    //   zone: hostedZoned,
    //   recordName: apiDnsName,
    //   target: RecordTarget.fromAlias(
    //     new LoadBalancerTarget(sharedAlbWithListener)
    //   ),
    // });
    // new ARecord(this, "InternalDnsRecord", {
    //   zone: hostedZoned,
    //   recordName: internalDnsName,
    //   target: RecordTarget.fromAlias(
    //     new LoadBalancerTarget(internalAlbWithListener)
    //   ),
    // });

    // var cloudfrontDistro = new Distribution(this, "distro", {
    //   defaultBehavior: {
    //     origin: new HttpOrigin(`${apiDnsName}.${dnsName}`, {
    //       customHeaders: {
    //         CloudFrontForwarded: "thisisacustomheader",
    //       },
    //     }),
    //     allowedMethods: AllowedMethods.ALLOW_ALL,
    //     responseHeadersPolicy: ResponseHeadersPolicy.CORS_ALLOW_ALL_ORIGINS,
    //     viewerProtocolPolicy: ViewerProtocolPolicy.HTTPS_ONLY,
    //     cachePolicy: CachePolicy.CACHING_DISABLED,
    //   },
    //   certificate: usEast1Cert,
    //   domainNames: [cloudfrontDnsName],
    // });

    // new ARecord(this, "CloudFrontDnsRecord", {
    //   zone: hostedZoned,
    //   recordName: cloudfrontDnsName,
    //   target: RecordTarget.fromAlias(new CloudFrontTarget(cloudfrontDistro)),
    // });

    // const httpListener = new ApplicationListener(this, "HttpListener", {
    //   loadBalancer: sharedAlbWithListener,
    //   port: 80,
    //   defaultAction: ListenerAction.fixedResponse(200),
    // });

    // const httpsListener = new ApplicationListener(this, "HttpsListener", {
    //   loadBalancer: sharedAlbWithListener,
    //   port: 443,
    //   defaultAction: ListenerAction.fixedResponse(200),
    // });

    // httpsListener.addCertificates("PlantBasedPizzaDomain", [certificate]);

    // const internalhttpListener = new ApplicationListener(
    //   this,
    //   "InternalHttpListener",
    //   {
    //     loadBalancer: sharedAlbWithListener,
    //     port: 80,
    //     defaultAction: ListenerAction.fixedResponse(200),
    //   }
    // );

    // const internalHttpsListener = new ApplicationListener(
    //   this,
    //   "InternalHttpsListener",
    //   {
    //     loadBalancer: internalAlbWithListener,
    //     port: 443,
    //     defaultAction: ListenerAction.fixedResponse(200),
    //   }
    // );

    // internalHttpsListener.addCertificates("PlantBasedPizzaDomain", [
    //   certificate,
    // ]);

    // const albEndpointParameter = new StringParameter(this, "ALBEndpointParam", {
    //   stringValue: `http://${sharedAlbWithListener.loadBalancerDnsName}`,
    //   parameterName: "/shared/alb-endpoint",
    // });

    // const albArnParameter = new StringParameter(this, "ALBArnParam", {
    //   stringValue: sharedAlbWithListener.loadBalancerArn,
    //   parameterName: "/shared/alb-arn",
    // });
    // const internalAlbEndpointParameter = new StringParameter(
    //   this,
    //   "InternalALBEndpointParam",
    //   {
    //     stringValue: `http://${internalAlbWithListener.loadBalancerDnsName}`,
    //     parameterName: "/shared/internal-alb-endpoint",
    //   }
    // );

    // const internalAlbArnParameter = new StringParameter(
    //   this,
    //   "InternalALBArnParam",
    //   {
    //     stringValue: internalAlbWithListener.loadBalancerArn,
    //     parameterName: "/shared/internal-alb-arn",
    //   }
    // );

    // const listenerArnParameter = new StringParameter(this, "ListenerArnParam", {
    //   stringValue: httpsListener.listenerArn,
    //   parameterName: "/shared/alb-listener",
    // });

    // const internalListenerArnParameter = new StringParameter(
    //   this,
    //   "InternalListenerArnParam",
    //   {
    //     stringValue: internalHttpsListener.listenerArn,
    //     parameterName: "/shared/internal-alb-listener",
    //   }
    // );
  }
}
