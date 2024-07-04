package com.recipes.infrastructure;

import software.amazon.awscdk.services.secretsmanager.ISecret;

public class BackgroundServiceProps {
    private final ISecret datadogKeyParameter;
    private final SharedProps sharedProps;
    
    public BackgroundServiceProps(SharedProps props, ISecret datadogKeyParameter) {
        this.datadogKeyParameter = datadogKeyParameter;
        this.sharedProps = props;
    }

    public ISecret getDatadogKeyParameter() {
        return datadogKeyParameter;
    }

    public SharedProps getSharedProps() {
        return sharedProps;
    }
}
