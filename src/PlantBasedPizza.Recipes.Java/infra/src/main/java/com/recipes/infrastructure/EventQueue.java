package com.recipes.infrastructure;

import org.jetbrains.annotations.NotNull;
import software.amazon.awscdk.Tags;
import software.amazon.awscdk.services.events.EventPattern;
import software.amazon.awscdk.services.events.Rule;
import software.amazon.awscdk.services.events.RuleProps;
import software.amazon.awscdk.services.events.targets.SqsQueue;
import software.amazon.awscdk.services.sqs.DeadLetterQueue;
import software.amazon.awscdk.services.sqs.IQueue;
import software.amazon.awscdk.services.sqs.Queue;
import software.amazon.awscdk.services.sqs.QueueProps;
import software.constructs.Construct;

import java.util.ArrayList;
import java.util.List;

public class EventQueue extends Construct {
    private final Queue deadLetterQueue;
    private final Queue queue;
    
    public EventQueue(@NotNull Construct scope, @NotNull String id, @NotNull EventQueueProps props) {
        super(scope, id);
        String eventSource = props.getEventSource();
        
        if (!eventSource.endsWith("/"))
        {
            eventSource = eventSource + "/";
        }
        
        String deadLetterQueueName = String.format("%sDLQ-%s", props.getQueueName(), props.getSharedProps().getEnvironment());
        
        this.deadLetterQueue = new Queue(this, deadLetterQueueName, 
                QueueProps.builder()
                        .queueName(deadLetterQueueName)
                        .build());

        String queueName = String.format("%s-%s", props.getQueueName(), props.getSharedProps().getEnvironment());
        
        this.queue = new Queue(this, queueName,
                QueueProps.builder()
                        .queueName(deadLetterQueueName)
                        .deadLetterQueue(DeadLetterQueue.builder()
                                .queue(this.deadLetterQueue)
                                .maxReceiveCount(3).build())
                        .build());

        Tags.of(this.queue).add("service", props.getSharedProps().getServiceName());
        Tags.of(this.deadLetterQueue).add("service", props.getSharedProps().getServiceName());

        Rule rule = new Rule(this, String.format("%sRule", props.getQueueName()),
                RuleProps.builder()
                        .eventBus(props.getBus())
                        .build());
        
        List<String> sources = new ArrayList<>();
        sources.add(eventSource);

        List<String> detailTypes = new ArrayList<>();
        detailTypes.add(props.getDetailType());
        
        EventPattern pattern = EventPattern
                .builder()
                .source(sources)
                .detailType(detailTypes)
                        .build();
        rule.addEventPattern(pattern);
        
        rule.addTarget(new SqsQueue(this.queue));
    }
    
    public IQueue getQueue() {
        return this.queue;
    }

    public IQueue getDeadLetterQueue() {
        return this.deadLetterQueue;
    }
}
