package com.recipes.infrastructure;

import software.amazon.awscdk.services.apigatewayv2.*;
import software.amazon.awscdk.services.ec2.*;
import software.amazon.awscdk.services.ecs.Cluster;
import software.amazon.awscdk.services.ecs.ClusterProps;
import software.amazon.awscdk.services.ecs.Secret;
import software.amazon.awscdk.services.events.EventBus;
import software.amazon.awscdk.services.events.IEventBus;
import software.amazon.awscdk.services.secretsmanager.ISecret;
import software.amazon.awscdk.services.servicediscovery.IPrivateDnsNamespace;
import software.amazon.awscdk.services.servicediscovery.PrivateDnsNamespace;
import software.amazon.awscdk.services.servicediscovery.PrivateDnsNamespaceAttributes;
import software.amazon.awscdk.services.ssm.IStringParameter;
import software.amazon.awscdk.services.ssm.SecureStringParameterAttributes;
import software.amazon.awscdk.services.ssm.StringParameter;
import software.amazon.awscdk.services.ssm.StringParameterAttributes;
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
        String namespaceId = StringParameter.valueFromLookup(this, "/shared/namespace-id");
        String namespaceName = StringParameter.valueFromLookup(this, "/shared/namespace-name");
        String namespaceArn = StringParameter.valueFromLookup(this, "/shared/namespace-arn");
        String httpApiId = StringParameter.valueFromLookup(this, "/shared/api-id");
        String internalHttpApiId = StringParameter.valueFromLookup(this, "/shared/internal-api-id");
        String vpcLinkId = StringParameter.valueFromLookup(this, "/shared/vpc-link-id");
        String vpcLinkSecurityGroupId = StringParameter.valueForStringParameter(this, "/shared/vpc-link-sg-id");

        String serviceName = "RecipeService";
        String environment = System.getenv("ENV") != null ? System.getenv("ENV") : "dev";
        String commitHash = System.getenv("COMMIT_HASH") != null ? System.getenv("COMMIT_HASH") : "latest";
        SharedProps sharedProps = new SharedProps(environment, serviceName, commitHash);

        // Lookup an existing VPC
        IVpc vpc = Vpc.fromLookup(this, "MainVpc", VpcLookupOptions.builder()
                .vpcId(vpcId)
                .build());

        IPrivateDnsNamespace serviceDiscoveryNamespace = PrivateDnsNamespace.fromPrivateDnsNamespaceAttributes(this, "DNSNamespace",
                PrivateDnsNamespaceAttributes.builder()
                        .namespaceId(namespaceId)
                        .namespaceArn(namespaceArn)
                        .namespaceName(namespaceName)
                        .build());

        IHttpApi httpApi = HttpApi.fromHttpApiAttributes(this, "HttpApi", HttpApiAttributes.builder()
                .httpApiId(httpApiId)
                .build());

        IHttpApi internalHttpApi = HttpApi.fromHttpApiAttributes(this, "InternalHttpApi", HttpApiAttributes.builder()
                .httpApiId(internalHttpApiId)
                .build());

        IVpcLink vpcLink = VpcLink.fromVpcLinkAttributes(this, "HttpApiVpcLink",
                VpcLinkAttributes.builder()
                        .vpcLinkId(vpcLinkId)
                        .vpc(vpc)
                        .build());
        
        SecureStringParameterAttributes ddApiKeyParameters = SecureStringParameterAttributes.builder()
                .parameterName("/shared/dd-api-key")
                .build();
        
        IStringParameter ddApiKeyParam = StringParameter.fromSecureStringParameterAttributes(this, "DDApiKey", ddApiKeyParameters);
        ISecret ddApiKeySecret = software.amazon.awscdk.services.secretsmanager.Secret.fromSecretNameV2(this, "DDApiKeySecret", "DdApiKeySecret-EAtKjZYFq40D");
        
        IEventBus bus = EventBus.fromEventBusName(this, "SharedEventBus", "PlantBasedPizzaEvents");

        Cluster cluster = new Cluster(this, "RecipeJavaCluster", ClusterProps.builder()
                .vpc(vpc)
                .enableFargateCapacityProviders(true)
                .build());

        HashMap<String, String> envVariables = new HashMap<>(1);
        envVariables.put("EVENT_BUS_NAME", bus.getEventBusName());
        envVariables.put("CACHE_NAME", "plant-based-pizza-recipes");

        HashMap<String, Secret> secretVariables = new HashMap<>(1);

        SecureStringParameterAttributes ddAttr = SecureStringParameterAttributes.builder()
                .parameterName("/recipe/db_conn_string")
                .build();
        IStringParameter connectionStringParam = StringParameter.fromSecureStringParameterAttributes(this, "ConnectionStringParam",  ddAttr);

        StringParameterAttributes momentoApiKeyAttributes = StringParameterAttributes.builder()
                .parameterName("/recipes/cache-api-key")
                .build();
        IStringParameter momentoApiKeyStringParameter = StringParameter.fromStringParameterAttributes(this, "MomentoApiKEyStringParam",  momentoApiKeyAttributes);

        secretVariables.put("DB_CONNECTION_STRING", Secret.fromSsmParameter(connectionStringParam));
        secretVariables.put("MOMENTO_API_KEY", Secret.fromSsmParameter(momentoApiKeyStringParameter));

        WebService javaWebService = new WebService(this, "JavaRecipeService", new WebServiceProps(
                vpc,
                vpcLink,
                vpcLinkSecurityGroupId,
                serviceDiscoveryNamespace,
                "recipe.api",
                httpApi,
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
                "/recipes/health",
                "/recipes/{proxy+}",
                true
                ));
        
        BackgroundServices services = new BackgroundServices(
                this, 
                "BackgroundServices", 
                new BackgroundServiceProps(sharedProps, ddApiKeySecret, connectionStringParam, momentoApiKeyStringParameter, bus, commitHash));
        
        connectionStringParam.grantRead(javaWebService.executionRole);
        bus.grantPutEventsTo(javaWebService.taskRole);
    }
}
