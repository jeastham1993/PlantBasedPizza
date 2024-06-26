package com.recipe.messaging;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategy;
import com.recipe.core.RecipeCreatedEventV1;
import com.recipe.core.RecipeDeletedEventV1;
import com.recipe.core.RecipeUpdatedEventV1;
import io.cloudevents.CloudEvent;
import io.cloudevents.core.builder.CloudEventBuilder;
import io.cloudevents.core.format.ContentType;
import io.cloudevents.core.provider.EventFormatProvider;
import io.cloudevents.jackson.JsonCloudEventData;
import io.opentracing.SpanContext;
import io.opentracing.util.GlobalTracer;
import org.springframework.stereotype.Service;
import software.amazon.awssdk.services.eventbridge.EventBridgeClient;
import software.amazon.awssdk.services.eventbridge.model.PutEventsRequest;
import software.amazon.awssdk.services.eventbridge.model.PutEventsRequestEntry;
import software.amazon.awssdk.services.eventbridge.model.PutEventsResponse;

import java.net.URI;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Service
public class EventBridgeEventPublisher {
    private final EventBridgeClient _eventBridge;
    private final ObjectMapper _mapper;
    private final String SOURCE = "https://recipes.plantbasedpizza";

    public EventBridgeEventPublisher(){
        _eventBridge = EventBridgeClient.builder()
                .build();
        _mapper = new ObjectMapper();
        _mapper.setPropertyNamingStrategy(PropertyNamingStrategy.LOWER_CAMEL_CASE);
    }

    public void publish(RecipeCreatedEventV1 evt) {
        try {
            this.publishEvent("recipe.recipeCreated.v1", _mapper.writeValueAsBytes(evt));
        }
        catch (JsonProcessingException e){
        }
    }

    public void publish(RecipeUpdatedEventV1 evt) {
        try {
            this.publishEvent("recipe.recipeUpdated.v1", _mapper.writeValueAsBytes(evt));
        }
        catch (JsonProcessingException e){
        }
    }

    public void publish(RecipeDeletedEventV1 evt) {
        try {
            this.publishEvent("recipe.recipeDeleted.v1", _mapper.writeValueAsBytes(evt));
        }
        catch (JsonProcessingException e){
        }
    }
    
    private void publishEvent(String type, byte[] contents) {
        SpanContext currentSpan = GlobalTracer.get().activeSpan().context();

        CloudEvent evtWrapper = CloudEventBuilder
                .v03()
                .withId(UUID.randomUUID().toString())
                .withType("recipe.recipeCreated.v1")
                .withSource(URI.create(this.SOURCE))
                .withData("application/json", contents)
                .withTime(OffsetDateTime.now())
                .withExtension("ddtraceid", currentSpan.toTraceId())
                .withExtension("ddspanid", currentSpan.toSpanId())
                .build();

        byte[] serialized = EventFormatProvider
                .getInstance()
                .resolveFormat(ContentType.JSON)
                .serialize(evtWrapper);

        PutEventsRequestEntry reqEntry = PutEventsRequestEntry.builder()
                .source(this.SOURCE)
                .detailType("recipe.recipeCreated.v1")
                .detail(new String(serialized))
                .eventBusName(System.getenv("EVENT_BUS_NAME"))
                .build();

        List<PutEventsRequestEntry> eventList = new ArrayList<>();
        eventList.add(reqEntry);

        PutEventsRequest eventsRequest = PutEventsRequest.builder()
                .entries(reqEntry)
                .build();

        PutEventsResponse result = _eventBridge.putEvents(eventsRequest);
    }
}
