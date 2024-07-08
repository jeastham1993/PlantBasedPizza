package com.recipes.infrastructure;

import software.amazon.awscdk.services.events.IEventBus;

public class EventQueueProps {
    
    private final String eventSource;
    private final String detailType;
    private final String queueName;
    private final SharedProps sharedProps;
    private final IEventBus bus;


    public EventQueueProps(SharedProps sharedProps, IEventBus bus, String eventSource, String detailType, String queueName) {
        this.eventSource = eventSource;
        this.sharedProps = sharedProps;
        this.detailType = detailType;
        this.queueName = queueName;
        this.bus = bus;
    }

    public String getEventSource() {
        return eventSource;
    }

    public SharedProps getSharedProps() {
        return sharedProps;
    }

    public String getQueueName() {
        return queueName;
    }

    public IEventBus getBus() {
        return bus;
    }

    public String getDetailType() {
        return detailType;
    }
}
