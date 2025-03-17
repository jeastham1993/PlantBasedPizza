package com.recipes.infrastructure;

import software.amazon.awscdk.services.apigatewayv2.IHttpApi;
import software.amazon.awscdk.services.apigatewayv2.IVpcLink;
import software.amazon.awscdk.services.ec2.IVpc;
import software.amazon.awscdk.services.ecs.ICluster;
import software.amazon.awscdk.services.ecs.Secret;
import software.amazon.awscdk.services.servicediscovery.INamespace;

import java.util.Map;

public class WebServiceProps{
    private final IVpc vpc;
    private final IVpcLink vpcLink;
    private final String vpcLinkSecurityGroupId;
    private final ICluster cluster;
    private final String serviceDiscoveryName;
    private final INamespace serviceDiscoveryNamespace;
    private final IHttpApi httpApi;
    private final String serviceName;
    private final String environment;
    private final String dataDogApiKeyParameterName;
    private final String jwtKeyParameterName;
    private final String repositoryName;
    private final String tag;
    private final int port;
    private final Map<String, String> environmentVariables;
    private final Map<String, Secret> secrets;
    private final String healthCheckPath;
    private final String pathPattern;
    private final boolean deployInPrivateSubnet;

    public WebServiceProps(IVpc vpc, IVpcLink vpcLink, String vpcLinkSecurityGroupId, INamespace serviceDiscoveryNamespace, String serviceDiscoveryName, IHttpApi httpApi, ICluster cluster, String serviceName, String environment, String dataDogApiKeyParameterName, String jwtKeyParameterName, String repositoryName, String tag, int port, Map<String, String> environmentVariables, Map<String, Secret> secrets, String healthCheckPath, String pathPattern, boolean deployInPrivateSubnet) {
        this.vpc = vpc;
        this.vpcLink = vpcLink;
        this.vpcLinkSecurityGroupId = vpcLinkSecurityGroupId;
        this.serviceDiscoveryName = serviceDiscoveryName;
        this.serviceDiscoveryNamespace = serviceDiscoveryNamespace;
        this.httpApi = httpApi;
        this.cluster = cluster;
        this.serviceName = serviceName;
        this.environment = environment;
        this.dataDogApiKeyParameterName = dataDogApiKeyParameterName;
        this.jwtKeyParameterName = jwtKeyParameterName;
        this.repositoryName = repositoryName;
        this.tag = tag;
        this.port = port;
        this.environmentVariables = environmentVariables;
        this.secrets = secrets;
        this.healthCheckPath = healthCheckPath;
        this.pathPattern = pathPattern;
        this.deployInPrivateSubnet = deployInPrivateSubnet;
    }

    public boolean isDeployInPrivateSubnet() {
        return deployInPrivateSubnet;
    }

    public String getPathPattern() {
        return pathPattern;
    }

    public String getHealthCheckPath() {
        return healthCheckPath;
    }

    public Map<String, Secret> getSecrets() {
        return secrets;
    }

    public int getPort() {
        return port;
    }

    public Map<String, String> getEnvironmentVariables() {
        return environmentVariables;
    }

    public String getTag() {
        return tag;
    }

    public String getRepositoryName() {
        return repositoryName;
    }

    public String getJwtKeyParameterName() {
        return jwtKeyParameterName;
    }

    public String getDataDogApiKeyParameterName() {
        return dataDogApiKeyParameterName;
    }

    public String getEnvironment() {
        return environment;
    }

    public String getServiceName() {
        return serviceName;
    }

    public ICluster getCluster() {
        return cluster;
    }

    public IVpc getVpc() {
        return vpc;
    }

    public IVpcLink getVpcLink() {
        return vpcLink;
    }

    public IHttpApi getHttpApi() {
        return httpApi;
    }

    public INamespace getServiceDiscoveryNamespace() {
        return serviceDiscoveryNamespace;
    }

    public String getServiceDiscoveryName() {
        return serviceDiscoveryName;
    }

    public String getVpcLinkSecurityGroupId() {
        return vpcLinkSecurityGroupId;
    }
}
