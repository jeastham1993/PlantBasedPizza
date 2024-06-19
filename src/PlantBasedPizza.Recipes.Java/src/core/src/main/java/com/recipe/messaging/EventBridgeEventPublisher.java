package com.recipe.messaging;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategy;
import com.recipe.core.RecipeCreatedEventV1;
import com.recipe.core.RecipeDeletedEventV1;
import com.recipe.core.RecipeUpdatedEventV1;
import org.springframework.stereotype.Service;
import software.amazon.awssdk.services.eventbridge.EventBridgeClient;
import software.amazon.awssdk.services.eventbridge.model.PutEventsRequest;
import software.amazon.awssdk.services.eventbridge.model.PutEventsRequestEntry;
import software.amazon.awssdk.services.eventbridge.model.PutEventsResponse;

import java.util.ArrayList;
import java.util.List;

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
            PutEventsRequestEntry reqEntry = PutEventsRequestEntry.builder()
                    .source(this.SOURCE)
                    .detailType("recipe.recipeCreated.v1")
                    .detail(_mapper.writeValueAsString(evt))
                    .eventBusName(System.getenv("EVENT_BUS_NAME"))
                    .build();

            List<PutEventsRequestEntry> eventList = new ArrayList<>();
            eventList.add(reqEntry);

            PutEventsRequest eventsRequest = PutEventsRequest.builder()
                    .entries(reqEntry)
                    .build();

            PutEventsResponse result = _eventBridge.putEvents(eventsRequest);
        }
        catch (JsonProcessingException e){
        }
    }

    public void publish(RecipeUpdatedEventV1 evt) {
        try {
            PutEventsRequestEntry reqEntry = PutEventsRequestEntry.builder()
                    .source(this.SOURCE)
                    .detailType("recipe.recipeUpdated.v1")
                    .detail(_mapper.writeValueAsString(evt))
                    .eventBusName(System.getenv("EVENT_BUS_NAME"))
                    .build();

            List<PutEventsRequestEntry> eventList = new ArrayList<>();
            eventList.add(reqEntry);

            PutEventsRequest eventsRequest = PutEventsRequest.builder()
                    .entries(reqEntry)
                    .build();

            PutEventsResponse result = _eventBridge.putEvents(eventsRequest);
        }
        catch (JsonProcessingException e){
        }
    }

    public void publish(RecipeDeletedEventV1 evt) {
        try {
            PutEventsRequestEntry reqEntry = PutEventsRequestEntry.builder()
                    .source(this.SOURCE)
                    .detailType("recipe.recipeDeleted.v1")
                    .detail(_mapper.writeValueAsString(evt))
                    .eventBusName(System.getenv("EVENT_BUS_NAME"))
                    .build();

            List<PutEventsRequestEntry> eventList = new ArrayList<>();
            eventList.add(reqEntry);

            PutEventsRequest eventsRequest = PutEventsRequest.builder()
                    .entries(reqEntry)
                    .build();

            PutEventsResponse result = _eventBridge.putEvents(eventsRequest);
        }
        catch (JsonProcessingException e){
        }
    }
}
