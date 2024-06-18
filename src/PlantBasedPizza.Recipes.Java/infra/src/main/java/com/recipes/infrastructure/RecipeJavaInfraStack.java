package com.recipes.infrastructure;

import org.jetbrains.annotations.NotNull;
import software.amazon.awscdk.services.ec2.*;
import software.amazon.awscdk.services.ecs.Cluster;
import software.amazon.awscdk.services.ecs.ClusterProps;
import software.amazon.awscdk.services.ecs.Secret;
import software.amazon.awscdk.services.elasticloadbalancingv2.ApplicationLoadBalancer;
import software.amazon.awscdk.services.elasticloadbalancingv2.ApplicationLoadBalancerLookupOptions;
import software.amazon.awscdk.services.elasticloadbalancingv2.IApplicationLoadBalancer;
import software.amazon.awscdk.services.events.EventBus;
import software.amazon.awscdk.services.events.IEventBus;
import software.amazon.awscdk.services.ssm.IStringParameter;
import software.amazon.awscdk.services.ssm.SecureStringParameterAttributes;
import software.amazon.awscdk.services.ssm.StringParameter;
import software.constructs.Construct;
import software.amazon.awscdk.Stack;
import software.amazon.awscdk.StackProps;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class RecipeJavaInfraStack extends Stack {
    public RecipeJavaInfraStack(final Construct scope, final String id) {
        this(scope, id, null);
    }

    public RecipeJavaInfraStack(final Construct scope, final String id, final StackProps props) {
        super(scope, id, props);

        String environment = System.getenv("ENV") != null ? System.getenv("ENV") : "dev";
        String commitHash = System.getenv("COMMIT_HASH") != null ? System.getenv("COMMIT_HASH") : "latest";

        IEventBus bus = EventBus.fromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        // Lookup an existing VPC
        IVpc vpc = Vpc.fromLookup(this, "MainVpc", VpcLookupOptions.builder()
                .vpcId("vpc-06c60c0d760921bc6")
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
                "JavaRecipeApi",
                environment,
                "/shared/dd-api-key",
                "/shared/jwt-key",
                "recipe-api-java",
                commitHash,
                8080,
                envVariables,
                secretVariables,
                "arn:aws:elasticloadbalancing:eu-west-1:730335273443:loadbalancer/app/plant-based-pizza-ingress/d99d1b57574af81c",
                "arn:aws:elasticloadbalancing:eu-west-1:730335273443:listener/app/plant-based-pizza-ingress/d99d1b57574af81c/d94d758d77bfc259",
                "/recipe/health",
                "/recipe/*",
                120,
                "arn:aws:elasticloadbalancing:eu-west-1:730335273443:loadbalancer/app/shared-internal-ingress/9de88d725cd4f625",
                "arn:aws:elasticloadbalancing:eu-west-1:730335273443:listener/app/shared-internal-ingress/9de88d725cd4f625/d9d8c7611b6f1d32",
                true
                ));

        connectionStringParam.grantRead(javaWebService.executionRole);
        bus.grantPutEventsTo(javaWebService.taskRole);
    }
}
