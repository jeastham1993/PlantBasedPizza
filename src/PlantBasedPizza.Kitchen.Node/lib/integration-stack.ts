import { CfnOutput, RemovalPolicy, Stack, StackProps } from "aws-cdk-lib";
import { Secret } from "aws-cdk-lib/aws-secretsmanager";
import { Construct } from "constructs";
import { Datadog } from "datadog-cdk-constructs-v2";
import { EventBus } from "aws-cdk-lib/aws-events";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { SharedProps } from "./constructs/sharedFunctionProps";
import { AttributeType, BillingMode, ProjectionType, Table, TableClass } from "aws-cdk-lib/aws-dynamodb";
import { Api } from "./api";
import { BackgroundWorker } from "./backgroundWorkers";
import { HttpApi } from "aws-cdk-lib/aws-apigatewayv2";

export class IntegrationTestStack extends Stack {
  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    const environment = process.env.ENV ?? "test";
    const serviceName = "KitchenService";
    const version = process.env.VERSION ?? "latest";
    const ddSecretName = process.env.DD_API_KEY_SECRET_NAME ?? "";

    var datadogConfiguration: Datadog | undefined = undefined;

    if (ddSecretName.length > 0){
      const ddApiKey = Secret.fromSecretNameV2(this, "DDApiKeySecret", ddSecretName);

      datadogConfiguration = new Datadog(this, "Datadog", {
        nodeLayerVersion: 112,
        extensionLayerVersion: 59,
        site: "datadoghq.eu",
        apiKeySecret: ddApiKey,
        service: "KitchenService",
        version: version,
        env: environment,
        enableColdStartTracing: true,
        captureLambdaPayload: environment == "prod" ? false : true,
        enableProfiling: true,
        enableDatadogASM: true
      });
    }

    const jwtKey = StringParameter.fromSecureStringParameterAttributes(
      this,
      "JwtKeyParam",
      {
        parameterName: "/shared/jwt-key",
      },
    );

    const eventBridge = new EventBus(this, "KitchenServiceTestBus", {
        eventBusName: `kitchen-service.${version}`
    });

    const table = new Table(this, `KitchenDataTable${version}`, {
      tableName: `kitchen-integration-test.${version}`,
      tableClass: TableClass.STANDARD,
      billingMode: BillingMode.PAY_PER_REQUEST,
      partitionKey: {
        name: 'PK',
        type: AttributeType.STRING
      },
      removalPolicy: RemovalPolicy.DESTROY
    });
    table.addGlobalSecondaryIndex({
      indexName: 'GSI1',
      projectionType: ProjectionType.ALL,
      partitionKey: {
        name: "GSI1PK",
        type: AttributeType.STRING
      },
      sortKey: {
        name: "GSI1SK",
        type: AttributeType.STRING
      }
    });

    const httpApi = new HttpApi(this, "KitchenIntegrationTestApi");

    const sharedProps: SharedProps = {
      serviceName,
      environment,
      version,
      apiProps: {
        // Add API Gateway resource
        apiGateway: httpApi,
        albListener: undefined
      },
      datadogConfiguration,
      table
    };

    const api = new Api(this, "KitchenApi", {
      sharedProps,
      bus: eventBridge,
      table,
      jwtKey
    });

    const backgroundWorkers = new BackgroundWorker(this, "BackgroundWorker", {
      sharedProps,
      table,
      bus: eventBridge
    });

    const apiUrlOutput = new CfnOutput(this, "ApiUrlOutput", {
      exportName: 'ApiUrl',
      value: `${httpApi.url!}kitchen`
    })
  }
}
