import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { Network } from './network';
import { Cluster, ContainerImage, CpuArchitecture, OperatingSystemFamily, Secret } from 'aws-cdk-lib/aws-ecs';
import { EventBus } from 'aws-cdk-lib/aws-events';
import { ApplicationListener, ApplicationLoadBalancer, ApplicationProtocol, ListenerAction, Protocol } from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import { ApplicationLoadBalancedFargateService } from 'aws-cdk-lib/aws-ecs-patterns';
import { StringParameter } from 'aws-cdk-lib/aws-ssm';

export class PlantBasedPizzaSharedInfrastructureStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const network = new Network(this, "PlantBasedPizzaNetworkResources");

    const sharedCluster = new Cluster(this, "PlantBasedPizzaSharedCluster", {
      vpc: network.vpc,
    });

    const sharedAlb = new ApplicationLoadBalancer(this, "ApplicationIngress", {
      loadBalancerName: "plant-based-pizza-shared-ingress",
      vpc: network.vpc,
      internetFacing: true
    });

    const sharedAlbWithListener = new ApplicationLoadBalancer(this, "ApplicationIngressWithListener", {
      loadBalancerName: "plant-based-pizza-ingress",
      vpc: network.vpc,
      internetFacing: true
    });

    const httpListener = new ApplicationListener(this, "HttpListener", {
      loadBalancer: sharedAlbWithListener,
      port: 80,
      defaultAction: ListenerAction.fixedResponse(200)
    });

    const eventBus = new EventBus(this, "PlantBasedPizzaEventBus", {
      eventBusName: 'PlantBasedPizzaEvents'
    })
  }
}
