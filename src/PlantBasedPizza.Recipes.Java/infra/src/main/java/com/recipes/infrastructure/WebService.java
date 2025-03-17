package com.recipes.infrastructure;

import org.jetbrains.annotations.NotNull;
import software.amazon.awscdk.Duration;
import software.amazon.awscdk.aws_apigatewayv2_integrations.HttpServiceDiscoveryIntegration;
import software.amazon.awscdk.aws_apigatewayv2_integrations.HttpServiceDiscoveryIntegrationProps;
import software.amazon.awscdk.services.apigatewayv2.HttpMethod;
import software.amazon.awscdk.services.apigatewayv2.HttpRoute;
import software.amazon.awscdk.services.apigatewayv2.HttpRouteKey;
import software.amazon.awscdk.services.apigatewayv2.HttpRouteProps;
import software.amazon.awscdk.services.ec2.ISecurityGroup;
import software.amazon.awscdk.services.ec2.Peer;
import software.amazon.awscdk.services.ec2.Port;
import software.amazon.awscdk.services.ec2.SubnetSelection;
import software.amazon.awscdk.services.ecr.IRepository;
import software.amazon.awscdk.services.ecr.Repository;
import software.amazon.awscdk.services.ecs.*;
import software.amazon.awscdk.services.ecs.Protocol;
import software.amazon.awscdk.services.iam.*;
import software.amazon.awscdk.services.servicediscovery.DnsRecordType;
import software.amazon.awscdk.services.servicediscovery.Service;
import software.amazon.awscdk.services.servicediscovery.ServiceProps;
import software.amazon.awscdk.services.ssm.IStringParameter;
import software.amazon.awscdk.services.ssm.SecureStringParameterAttributes;
import software.amazon.awscdk.services.ssm.StringParameter;
import software.constructs.Construct;

import java.util.*;

public class WebService extends Construct {

    public final IRole executionRole;
    public final IRole taskRole;

    public WebService(@NotNull Construct scope, @NotNull String id, @NotNull WebServiceProps props) {
        super(scope, id);

        SecureStringParameterAttributes ddAttr = SecureStringParameterAttributes.builder()
                .parameterName(props.getDataDogApiKeyParameterName())
                .build();

        SecureStringParameterAttributes jwtAttr = SecureStringParameterAttributes.builder()
                .parameterName(props.getJwtKeyParameterName())
                .build();

        IStringParameter ddApiKeyParam = StringParameter.fromSecureStringParameterAttributes(this, "DDApiKey",  ddAttr);

        IStringParameter jwtKeyParam = StringParameter.fromSecureStringParameterAttributes(this, "JWTKeyParam", jwtAttr);

        IRepository repository = Repository.fromRepositoryName(this, props.getServiceName() + "Repo", props.getRepositoryName());

        executionRole = new Role(this, props.getServiceName() + "ExecutionRole", RoleProps.builder().assumedBy(new ServicePrincipal("ecs-tasks.amazonaws.com")).build());
        executionRole.addManagedPolicy(ManagedPolicy.fromManagedPolicyArn(this, "TaskExecutionPolicy", "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"));

        taskRole = new Role(this, props.getServiceName() + "TaskRole", RoleProps.builder().assumedBy(new ServicePrincipal("ecs-tasks.amazonaws.com")).build());

        Map<String, String> baseEnvironmentVariables = new HashMap<>();
        baseEnvironmentVariables.put("OtlpUseHttp", "Y");
        baseEnvironmentVariables.put("Environment", props.getEnvironment());
        baseEnvironmentVariables.put("ServiceDiscovery__MyUrl", "");
        baseEnvironmentVariables.put("ServiceDiscovery__ServiceName", "");
        baseEnvironmentVariables.put("ServiceDiscovery__ConsulServiceEndpoint", "");
        baseEnvironmentVariables.put("Auth__Issuer", "https://plantbasedpizza.com");
        baseEnvironmentVariables.put("Auth__Audience", "https://plantbasedpizza.com");
        baseEnvironmentVariables.put("ECS_ENABLE_CONTAINER_METADATA", "true");
        baseEnvironmentVariables.put("ENV", props.getEnvironment());
        baseEnvironmentVariables.put("DD_ENV", props.getEnvironment());
        baseEnvironmentVariables.put("DD_SERVICE", props.getServiceName());
        baseEnvironmentVariables.put("DD_VERSION", props.getTag());
        baseEnvironmentVariables.put("DD_GIT_COMMIT_SHA", props.getTag());
        baseEnvironmentVariables.put("DD_GIT_REPOSITORY_URL", "https://github.com/jeastham1993/PlantBasedPizza");
        baseEnvironmentVariables.put("DD_AGENT_HOST", "127.0.0.1");
        baseEnvironmentVariables.put("DD_TRACE_ROUTE_TEMPLATE_RESOURCE_NAMES_ENABLED", "true");
        baseEnvironmentVariables.put("DD_RUNTIME_METRICS_ENABLED", "true");
        baseEnvironmentVariables.put("DD_APM_IGNORE_RESOURCES", String.format("(GET) %s", props.getHealthCheckPath()));
        baseEnvironmentVariables.put("DD_IAST_ENABLED", "true");

        Map<String, Secret> baseSecrets = new HashMap<>();
        baseSecrets.put("JWT_KEY", Secret.fromSsmParameter(jwtKeyParam));

        FargateTaskDefinitionProps taskDefinitionProps = FargateTaskDefinitionProps.builder()
                .memoryLimitMiB(2048)
                .cpu(512)
                .runtimePlatform(RuntimePlatform.builder().cpuArchitecture(CpuArchitecture.X86_64).operatingSystemFamily(OperatingSystemFamily.LINUX).build())
                .executionRole(executionRole)
                .taskRole(taskRole)
                .build();

        FargateTaskDefinition taskDefinition = new FargateTaskDefinition(this, props.getServiceName() + "Definition", taskDefinitionProps);

        ArrayList<PortMapping> portMappingList = new ArrayList<PortMapping>(1);
        portMappingList.add(PortMapping.builder().containerPort(props.getPort()).protocol(Protocol.TCP).build());

        Map<String, String> options = new HashMap<>();
        options.put("Name", "datadog");
        options.put("Host", "http-intake.logs.datadoghq.eu");
        options.put("TLS", "on");
        options.put("dd_service", props.getServiceName());
        options.put("dd_source", "aspnet");
        options.put("dd_message_key", "log");
        options.put("dd_tags", "project:" + props.getServiceName());
        options.put("provider", "ecs");

        Map<String, Secret> secretOptions = new HashMap<>();
        secretOptions.put("apikey", Secret.fromSsmParameter(ddApiKeyParam));

        FireLensLogDriverProps fireLensLogDriverProps = FireLensLogDriverProps.builder()
                .options(options)
                .secretOptions(secretOptions)
                .build();

        baseSecrets.putAll(props.getSecrets());
        baseEnvironmentVariables.putAll(props.getEnvironmentVariables());

        ContainerDefinitionOptions containerDefinitionOptions = ContainerDefinitionOptions.builder()
                .image(ContainerImage.fromEcrRepository(repository, props.getTag() != null ? props.getTag() : "latest"))
                .portMappings(portMappingList)
                .containerName(props.getServiceName())
                .environment(baseEnvironmentVariables)
                .secrets(baseSecrets)
                .logging(LogDrivers.firelens(fireLensLogDriverProps))
                .build();

        ContainerDefinition container = taskDefinition.addContainer("application", containerDefinitionOptions);

        ArrayList<PortMapping> datadogPortMappings = new ArrayList<PortMapping>(1);
        portMappingList.add(PortMapping.builder().containerPort(4317).build());
        portMappingList.add(PortMapping.builder().containerPort(4318).build());
        portMappingList.add(PortMapping.builder().containerPort(8126).build());

        Map<String, String> datadogEnvVars = new HashMap<>();
        datadogEnvVars.put("DD_SITE", "datadoghq.eu");
        datadogEnvVars.put("ECS_FARGATE", "true");
        datadogEnvVars.put("DD_LOGS_ENABLED", "false");
        datadogEnvVars.put("DD_PROCESS_AGENT_ENABLED", "true");
        datadogEnvVars.put("DD_APM_ENABLED", "true");
        datadogEnvVars.put("DD_APM_NON_LOCAL_TRAFFIC", "true");
        datadogEnvVars.put("DD_DOGSTATSD_NON_LOCAL_TRAFFIC", "true");
        datadogEnvVars.put("DD_ENV", props.getEnvironment());
        datadogEnvVars.put("DD_SERVICE", props.getServiceName());
        datadogEnvVars.put("DD_VERSION", props.getTag());
        datadogEnvVars.put("DD_APM_IGNORE_RESOURCES", String.format("(GET) %s", props.getHealthCheckPath()));

        Map<String, Secret> datadogSecrets = new HashMap<>();
        datadogSecrets.put("DD_API_KEY", Secret.fromSsmParameter(ddApiKeyParam));

        taskDefinition.addContainer("datadog-agent", ContainerDefinitionOptions.builder()
                        .image(ContainerImage.fromRegistry("public.ecr.aws/datadog/agent:latest"))
                        .portMappings(datadogPortMappings)
                        .containerName("datadog-agent")
                        .environment(datadogEnvVars)
                        .secrets(datadogSecrets)
                .build());

        taskDefinition.addFirelensLogRouter("firelens", FirelensLogRouterDefinitionOptions.builder()
                        .essential(true)
                        .image(ContainerImage.fromRegistry("amazon/aws-for-fluent-bit:stable"))
                        .containerName("log-router")
                        .firelensConfig(FirelensConfig.builder()
                                .type(FirelensLogRouterType.FLUENTBIT)
                                .options(FirelensOptions.builder()
                                        .enableEcsLogMetadata(true)
                                        .build())
                                .build())
                .build());

        Service cloudMapService = new Service(this, "CloudMapService", ServiceProps.builder()
                .namespace(props.getServiceDiscoveryNamespace())
                .name(props.getServiceDiscoveryName())
                .dnsTtl(Duration.seconds(60))
                .dnsRecordType(DnsRecordType.SRV)
                .build());

        // Fargate Service
        FargateService service = new FargateService(this, "FargateWebService",
                FargateServiceProps
                        .builder()
                        .cluster(props.getCluster())
                        .taskDefinition(taskDefinition)
                        .desiredCount(1)
                        .assignPublicIp(!props.isDeployInPrivateSubnet())
                        .enableExecuteCommand(true)
                        .vpcSubnets(SubnetSelection.builder()
                                .subnets(props.isDeployInPrivateSubnet() ? props.getVpc().getPrivateSubnets() : props.getVpc().getPublicSubnets())
                                .build())
                .build());
        service.associateCloudMapService(AssociateCloudMapServiceOptions.builder()
                .service(cloudMapService)
                .build());

        for (ISecurityGroup securityGroup : service.getConnections().getSecurityGroups()) {
            securityGroup.addIngressRule(Peer.securityGroupId(props.getVpcLinkSecurityGroupId()), Port.tcp(props.getPort()));
        }

        HttpServiceDiscoveryIntegration serviceDiscoveryIntegration = new HttpServiceDiscoveryIntegration("ApplicationServiceDiscovery",
                cloudMapService,
                HttpServiceDiscoveryIntegrationProps.builder()
                        .method(HttpMethod.ANY)
                        .vpcLink(props.getVpcLink())
                        .build());

        HttpRoute route = new HttpRoute(this, "ProxyRoute", HttpRouteProps.builder()
                .httpApi(props.getHttpApi())
                .routeKey(HttpRouteKey.with(props.getPathPattern(), HttpMethod.ANY))
                .integration(serviceDiscoveryIntegration)
                .build());

    }
}