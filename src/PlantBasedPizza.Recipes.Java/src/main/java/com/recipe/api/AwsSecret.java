package com.recipe.api;

public class AwsSecret {
    private String username;
    private String password;
    private String host;
    private String engine;
    private String port;
    private String dbInstanceIdentifier;

    public String getUsername() {
        return username;
    }

    public String getPassword() {
        return password;
    }

    public String getHost() {
        return host;
    }

    public String getEngine() {
        return engine;
    }

    public String getPort() {
        return port;
    }

    public String getDbInstanceIdentifier() {
        return dbInstanceIdentifier;
    }
}
