import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { Network } from './network';
import { EventBus } from 'aws-cdk-lib/aws-events';
import { ApplicationListener, ApplicationLoadBalancer, ListenerAction} from 'aws-cdk-lib/aws-elasticloadbalancingv2';

export class PlantBasedPizzaSharedInfrastructureStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const network = new Network(this, "PlantBasedPizzaNetworkResources");

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
