package com.recipes.infrastructure;

import software.amazon.awscdk.services.ec2.*;
import software.amazon.awscdk.services.ecs.Cluster;
import software.amazon.awscdk.services.ecs.ClusterProps;
import software.amazon.awscdk.services.ecs.Secret;
import software.amazon.awscdk.services.events.EventBus;
import software.amazon.awscdk.services.events.IEventBus;
import software.amazon.awscdk.services.secretsmanager.ISecret;
import software.amazon.awscdk.services.ssm.IStringParameter;
import software.amazon.awscdk.services.ssm.SecureStringParameterAttributes;
import software.amazon.awscdk.services.ssm.StringParameter;
import software.constructs.Construct;
import software.amazon.awscdk.Stack;
import software.amazon.awscdk.StackProps;

import java.util.HashMap;

public class RecipeJavaInfraStack extends Stack {
    public RecipeJavaInfraStack(final Construct scope, final String id) {
        this(scope, id, null);
    }

    public RecipeJavaInfraStack(final Construct scope, final String id, final StackProps props) {
        super(scope, id, props);

        String vpcId = StringParameter.valueFromLookup(this, "/shared/vpc-id");
        String albArn = StringParameter.valueFromLookup(this, "/shared/alb-arn");
        String albListenerArn = StringParameter.valueFromLookup(this, "/shared/alb-listener");
        String internalAlbArn = StringParameter.valueFromLookup(this, "/shared/internal-alb-arn");
        String internalAlbListenerArn = StringParameter.valueFromLookup(this, "/shared/internal-alb-listener");
        String serviceName = "RecipeService";
        String environment = System.getenv("ENV") != null ? System.getenv("ENV") : "dev";
        String commitHash = System.getenv("COMMIT_HASH") != null ? System.getenv("COMMIT_HASH") : "latest";
        SharedProps sharedProps = new SharedProps(environment, serviceName, commitHash);
        
        SecureStringParameterAttributes ddApiKeyParameters = SecureStringParameterAttributes.builder()
                .parameterName("/shared/dd-api-key")
                .build();
        
        IStringParameter ddApiKeyParam = StringParameter.fromSecureStringParameterAttributes(this, "DDApiKey", ddApiKeyParameters);
        ISecret ddApiKeySecret = software.amazon.awscdk.services.secretsmanager.Secret.fromSecretNameV2(this, "DDApiKeySecret", "DdApiKeySecret-EAtKjZYFq40D");
        
        IEventBus bus = EventBus.fromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        // Lookup an existing VPC
        IVpc vpc = Vpc.fromLookup(this, "MainVpc", VpcLookupOptions.builder()
                .vpcId(vpcId)
                .build());

        Cluster cluster = new Cluster(this, "RecipeJavaCluster", ClusterProps.builder()
                .vpc(vpc)
                .enableFargateCapacityProviders(true)
                .build());

        HashMap<String, String> envVariables = new HashMap<>(1);
        envVariables.put("EVENT_BUS_NAME", bus.getEventBusName());

        HashMap<String, Secret> secretVariables = new HashMap<>(1);

        SecureStringParameterAttributes ddAttr = SecureStringParameterAttributes.builder()
                .parameterName("/recipe/db_conn_string")
                .build();
        IStringParameter connectionStringParam = StringParameter.fromSecureStringParameterAttributes(this, "ConnectionStringParam",  ddAttr);

        secretVariables.put("DB_CONNECTION_STRING", Secret.fromSsmParameter(connectionStringParam));

        WebService javaWebService = new WebService(this, "JavaRecipeService", new WebServiceProps(
                vpc,
                cluster,
                serviceName,
                environment,
                "/shared/dd-api-key",
                "/shared/jwt-key",
                "recipe-api-java",
                commitHash,
                8080,
                envVariables,
                secretVariables,
                albArn,
                albListenerArn,
                "/recipes/health",
                "/recipes/*",
                122,
                internalAlbArn,
                internalAlbListenerArn,
                true
                ));
        
        BackgroundServices services = new BackgroundServices(
                this, 
                "BackgroundServices", 
                new BackgroundServiceProps(sharedProps, ddApiKeySecret));
        
        connectionStringParam.grantRead(javaWebService.executionRole);
        bus.grantPutEventsTo(javaWebService.taskRole);
    }
}
