package com.recipes.infrastructure;

import software.amazon.awscdk.services.ec2.IVpc;
import software.amazon.awscdk.services.ecs.ICluster;
import software.amazon.awscdk.services.ecs.Secret;

import java.util.Map;

public class WebServiceProps{
    private final IVpc vpc;
    private final ICluster cluster;
    private final String serviceName;
    private final String environment;
    private final String dataDogApiKeyParameterName;
    private final String jwtKeyParameterName;
    private final String repositoryName;
    private final String tag;
    private final int port;
    private final Map<String, String> environmentVariables;
    private final Map<String, Secret> secrets;
    private final String sharedLoadBalancerArn;
    private final String sharedListenerArn;
    private final String healthCheckPath;
    private final String pathPattern;
    private final int priority;
    private final String internalSharedLoadBalancerArn;
    private final String internalSharedListenerArn;
    private final boolean deployInPrivateSubnet;

    public WebServiceProps(IVpc vpc, ICluster cluster, String serviceName, String environment, String dataDogApiKeyParameterName, String jwtKeyParameterName, String repositoryName, String tag, int port, Map<String, String> environmentVariables, Map<String, Secret> secrets, String sharedLoadBalancerArn, String sharedListenerArn, String healthCheckPath, String pathPattern, int priority, String internalSharedLoadBalancerArn, String internalSharedListenerArn, boolean deployInPrivateSubnet) {
        this.vpc = vpc;
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
        this.sharedLoadBalancerArn = sharedLoadBalancerArn;
        this.sharedListenerArn = sharedListenerArn;
        this.healthCheckPath = healthCheckPath;
        this.pathPattern = pathPattern;
        this.priority = priority;
        this.internalSharedLoadBalancerArn = internalSharedLoadBalancerArn;
        this.internalSharedListenerArn = internalSharedListenerArn;
        this.deployInPrivateSubnet = deployInPrivateSubnet;
    }

    public boolean isDeployInPrivateSubnet() {
        return deployInPrivateSubnet;
    }

    public String getInternalSharedListenerArn() {
        return internalSharedListenerArn;
    }

    public String getInternalSharedLoadBalancerArn() {
        return internalSharedLoadBalancerArn;
    }

    public int getPriority() {
        return priority;
    }

    public String getPathPattern() {
        return pathPattern;
    }

    public String getHealthCheckPath() {
        return healthCheckPath;
    }

    public String getSharedListenerArn() {
        return sharedListenerArn;
    }

    public String getSharedLoadBalancerArn() {
        return sharedLoadBalancerArn;
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
}
