package com.recipes.infrastructure;

import software.amazon.awscdk.services.events.IEventBus;
import software.amazon.awscdk.services.secretsmanager.ISecret;

public class BackgroundServiceProps {
    private final ISecret datadogKeyParameter;
    private final SharedProps sharedProps;
    private final IEventBus bus;
    
    public BackgroundServiceProps(SharedProps props, ISecret datadogKeyParameter, IEventBus bus) {
        this.datadogKeyParameter = datadogKeyParameter;
        this.sharedProps = props;
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
}
