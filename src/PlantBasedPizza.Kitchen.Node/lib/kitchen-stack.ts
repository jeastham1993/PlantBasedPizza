import { Stack, StackProps } from "aws-cdk-lib";
import { Secret } from "aws-cdk-lib/aws-secretsmanager";
import { Construct } from "constructs";
import { Datadog } from "datadog-cdk-constructs-v2";
import { ApplicationListener } from "aws-cdk-lib/aws-elasticloadbalancingv2";
import { Vpc } from "aws-cdk-lib/aws-ec2";
import { InstrumentedApiLambdaFunction } from "./constructs/lambdaFunction";
import { InstrumentedSqsLambdaFunction } from "./constructs/sqsLambdaFunction";
import { EventBus } from "aws-cdk-lib/aws-events";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { SharedFunctionProps } from "./constructs/sharedFunctionProps";

export class KitchenStack extends Stack {
  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    const vpcIdParam = StringParameter.valueFromLookup(this, "/shared/vpc-id");
    const albArnParam = StringParameter.valueFromLookup(this, "/shared/alb-arn");
    const albListenerParam = StringParameter.valueFromLookup(this, "/shared/alb-listener");
    const environment = process.env.ENV ?? "test";
    const serviceName = "KitchenService";
    const version = process.env.COMMIT_HASH ?? "latest";

    const vpc = Vpc.fromLookup(this, "Vpc", {
      vpcId: vpcIdParam,
    });

    const ddApiKey = Secret.fromSecretNameV2(this, "DDApiKeySecret", "DdApiKeySecret-EAtKjZYFq40D");

    const databaseConnectionParam = StringParameter.fromSecureStringParameterAttributes(
      this,
      "DatabaseConnectionParam",
      {
        parameterName: "/shared/database-connection",
      },
    );

    const eventBridge = EventBus.fromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

    const datadogConfiguration = new Datadog(this, "Datadog", {
      nodeLayerVersion: 112,
      extensionLayerVersion: 58,
      site: "datadoghq.eu",
      apiKeySecret: ddApiKey,
      service: "KitchenService",
      version: process.env["COMMIT_HASH"] ?? "latest",
      env: environment,
      enableColdStartTracing: true,
    });

    const albListener = ApplicationListener.fromLookup(this, "SharedHttpListener", {
      loadBalancerArn: albArnParam,
      listenerArn: albListenerParam,
    });

    const sharedProps: SharedFunctionProps = {
      serviceName,
      environment,
      version,
      vpc,
      albListener,
      databaseConnectionParam,
      datadogConfiguration,
    } 

    const getNewFunction = new InstrumentedApiLambdaFunction(this, "GetNewFunction", {
      sharedProps,
      entry: "./src/lambda/getNew.ts",
      functionName: "GetNewFunction",
      path: "/kitchen/new",
      methods: ['GET'],
      priority: 36,
    });
    const getPrepCompleteFunction = new InstrumentedApiLambdaFunction(this, "GetPrepCompleteFunction", {
      sharedProps,
      entry: "./src/lambda/getPreparing.ts",
      functionName: "GetPrepCompleteFunction",
      path: "/kitchen/prep",
      methods: ['GET'],
      priority: 33,
    });
    const getBakingFunction = new InstrumentedApiLambdaFunction(this, "GetBakingFunction", {
      sharedProps,
      entry: "./src/lambda/getBaking.ts",
      functionName: "GetBakingFunction",
      path: "/kitchen/baking",
      methods: ['GET'],
      priority: 34,
    });
    const getAwaitingQualityCheckFunction = new InstrumentedApiLambdaFunction(this, "GetAwaitingQualityCheckFunction", {
      sharedProps,
      entry: "./src/lambda/getAwaitingQualityCheck.ts",
      functionName: "GetAwaitingQualityCheckFunction",
      path: "/kitchen/quality-check",
      methods: ['GET'],
      priority: 35,
    });

    const orderConfirmedHandler = new InstrumentedSqsLambdaFunction(this, "HandleOrderConfirmedEvent", {
      sharedProps,
      entry: "./src/lambda/handleOrderConfirmedEvent.ts",
      functionName: "HandleOrderConfirmedEvent",
    });
    orderConfirmedHandler.function.addEnvironment("BUS_NAME", eventBridge.eventBusName);
    eventBridge.grantPutEventsTo(orderConfirmedHandler.function);
  }
}
