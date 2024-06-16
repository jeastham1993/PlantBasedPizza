package com.recipe.api;

public class ApplicationProperties {
    private String databaseEndpoint;

    public String getDatabaseEndpoint()
    {
        return databaseEndpoint;
    }

    public void setMyApplicationProperty(String propertyValue)
    {
        this.databaseEndpoint = propertyValue;
    }
}
