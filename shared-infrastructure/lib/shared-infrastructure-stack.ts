import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { Network } from './network';
import { EventBus } from 'aws-cdk-lib/aws-events';
import { ApplicationListener, ApplicationLoadBalancer, ListenerAction} from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import { StringParameter } from 'aws-cdk-lib/aws-ssm';
import { ARecord, HostedZone, RecordTarget } from 'aws-cdk-lib/aws-route53';
import { Certificate } from 'aws-cdk-lib/aws-certificatemanager';
import { LoadBalancerTarget } from 'aws-cdk-lib/aws-route53-targets';

export class PlantBasedPizzaSharedInfrastructureStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const network = new Network(this, "PlantBasedPizzaNetworkResources");
    const dnsName = process.env['DNS_NAME']!
    const certArn = process.env['CERT_ARN']!;
    const hostedZoneId = process.env['HOSTED_ZONE_ID']!;

    const hostedZoned = HostedZone.fromHostedZoneAttributes(
      this,
      "HostedZone",
      {
        zoneName: dnsName,
        hostedZoneId: hostedZoneId,
      }
     );

    const certificate = Certificate.fromCertificateArn(this, 'PlantBasedPizzaCert', certArn);
    
    const sharedAlbWithListener = new ApplicationLoadBalancer(this, "ApplicationIngressWithListener", {
      loadBalancerName: "plant-based-pizza-ingress",
      vpc: network.vpc,
      internetFacing: true,
    });

    new ARecord(this, "DnsRecord", {
      zone: hostedZoned,
      recordName: dnsName,
      target: RecordTarget.fromAlias(new LoadBalancerTarget(sharedAlbWithListener)),
     });

    const httpListener = new ApplicationListener(this, "HttpListener", {
      loadBalancer: sharedAlbWithListener,
      port: 80,
      defaultAction: ListenerAction.fixedResponse(200)
    });
    const httpsListener = new ApplicationListener(this, "HttpsListener", {
      loadBalancer: sharedAlbWithListener,
      port: 443,
      defaultAction: ListenerAction.fixedResponse(200)
    });

    httpsListener.addCertificates('PlantBasedPizzaDomain', [
      certificate
    ]);


    const internalSharedAlbWithListener = new ApplicationLoadBalancer(this, "InternalApplicationIngressWithListener", {
      loadBalancerName: "shared-internal-ingress",
      vpc: network.vpc,
      internetFacing: false
    });
    const internalHttpListener = new ApplicationListener(this, "InternalHttpListener", {
      loadBalancer: internalSharedAlbWithListener,
      port: 80,
      defaultAction: ListenerAction.fixedResponse(200)
    });

    const eventBus = new EventBus(this, "PlantBasedPizzaEventBus", {
      eventBusName: 'PlantBasedPizzaEvents'
    })
    
    const albEndpointParameter = new StringParameter(this, "ALBEndpointParam", {
      stringValue: `http://${sharedAlbWithListener.loadBalancerDnsName}`,
      parameterName: '/shared/alb-endpoint'
    });

    const albArnParameter = new StringParameter(this, "ALBArnParam", {
      stringValue: sharedAlbWithListener.loadBalancerArn,
      parameterName: '/shared/alb-arn'
    });

    const listenerArnParameter = new StringParameter(this, "ListenerArnParam", {
      stringValue: httpsListener.listenerArn,
      parameterName: '/shared/alb-listener'
    });
    
    const internalAlbEndpointParameter = new StringParameter(this, "InternalALBEndpointParam", {
      stringValue: `http://${internalSharedAlbWithListener.loadBalancerDnsName}`,
      parameterName: '/shared/internal-alb-endpoint'
    });

    const internalAlbArnParameter = new StringParameter(this, "InternalALBArnParam", {
      stringValue: internalSharedAlbWithListener.loadBalancerArn,
      parameterName: '/shared/internal-alb-arn'
    });

    const internalListenerArnParameter = new StringParameter(this, "InternalListenerArnParam", {
      stringValue: internalHttpListener.listenerArn,
      parameterName: '/shared/internal-alb-listener'
    });

    const eventBusName = new StringParameter(this, "EventBusNameParam", {
      stringValue: eventBus.eventBusName,
      parameterName: '/shared/eb-name'
    });

    const ebArnParam = new StringParameter(this, "EventBusArnParam", {
      stringValue: eventBus.eventBusArn,
      parameterName: '/shared/eb-arn'
    });
  }
}
