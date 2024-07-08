package com.recipes.infrastructure;

public class SharedProps {
    private final String environment;
    private final String serviceName;
    private final String version;

    public SharedProps(String environment, String serviceName, String version) {
        this.environment = environment;
        this.serviceName = serviceName;
        this.version = version;
    }

    public String getVersion() {
        return version;
    }

    public String getServiceName() {
        return serviceName;
    }

    public String getEnvironment() {
        return environment;
    }
}
