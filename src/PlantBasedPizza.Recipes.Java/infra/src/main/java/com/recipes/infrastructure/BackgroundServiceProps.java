package com.recipes.infrastructure;

import software.amazon.awscdk.services.events.IEventBus;
import software.amazon.awscdk.services.secretsmanager.ISecret;
import software.amazon.awscdk.services.ssm.IStringParameter;

public class BackgroundServiceProps {
    private final ISecret datadogKeyParameter;
    private final SharedProps sharedProps;
    private final IStringParameter dbConnectionParameter;
    private final IStringParameter momentoApiKey;
    private final IEventBus bus;
    private final String tag;
    
    public BackgroundServiceProps(SharedProps props, ISecret datadogKeyParameter, IStringParameter dbConnectionParameter, IStringParameter momentoApiKey, IEventBus bus, String tag) {
        this.datadogKeyParameter = datadogKeyParameter;
        this.sharedProps = props;
        this.dbConnectionParameter = dbConnectionParameter;
        this.momentoApiKey = momentoApiKey;
        this.bus = bus;
        this.tag = tag;
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

    public String getTag() {
        return tag;
    }

    public IStringParameter getMomentoApiKey() {
        return momentoApiKey;
    }
}
