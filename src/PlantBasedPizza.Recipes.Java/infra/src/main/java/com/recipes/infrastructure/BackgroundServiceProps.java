package com.recipes.infrastructure;

import software.amazon.awscdk.services.events.IEventBus;
import software.amazon.awscdk.services.secretsmanager.ISecret;
import software.amazon.awscdk.services.ssm.IStringParameter;

public class BackgroundServiceProps {
    private final ISecret datadogKeyParameter;
    private final SharedProps sharedProps;
    private final IStringParameter dbConnectionParameter;
    private final IEventBus bus;
    
    public BackgroundServiceProps(SharedProps props, ISecret datadogKeyParameter, IStringParameter dbConnectionParameter, IEventBus bus) {
        this.datadogKeyParameter = datadogKeyParameter;
        this.sharedProps = props;
        this.dbConnectionParameter = dbConnectionParameter;
        this.bus = bus;
    }

    public ISecret getDatadogKeyParameter() {
        return datadogKeyParameter;
    }

    public SharedProps getSharedProps() {
        return sharedProps;
    }

    public IEventBus getBus() {
        return bus;
    }

    public IStringParameter getDbConnectionParameter() {
        return dbConnectionParameter;
    }
}
