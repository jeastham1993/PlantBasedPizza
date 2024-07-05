package com.recipe.functions;

import com.amazonaws.services.lambda.runtime.events.SQSEvent;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.recipe.functions.events.OrderConfirmedEvent;
import io.cloudevents.CloudEvent;
import io.cloudevents.core.data.PojoCloudEventData;
import io.cloudevents.core.format.EventFormat;
import io.cloudevents.core.provider.EventFormatProvider;
import io.cloudevents.jackson.JsonFormat;
import io.cloudevents.jackson.PojoCloudEventDataMapper;
import io.opentracing.Span;
import io.opentracing.util.GlobalTracer;

import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;

import static io.cloudevents.core.CloudEventUtils.mapData;

public class EventWrapper {
    private final ObjectMapper mapper;
    private final CloudEvent event;
    
    public EventWrapper(ObjectMapper mapper, SQSEvent.SQSMessage message) throws JsonProcessingException {
        this.mapper = mapper;
        HashMap wrapper = mapper.readValue(message.getBody(), HashMap.class);

        EventFormat format = EventFormatProvider
                .getInstance()
                .resolveFormat(JsonFormat.CONTENT_TYPE);

        Object value = wrapper.get("detail");

        String valueString = mapper.writeValueAsString(value);

        this.event = format.deserialize(valueString.getBytes(StandardCharsets.UTF_8));

        final Span span = GlobalTracer.get().activeSpan();
        
        if (span != null){
            span.setTag("linked.traceId", this.getParentTraceId());
            span.setTag("linked.spanId", this.getParentSpanId());
        }
    }
    
    public OrderConfirmedEvent AsOrderConfirmedEvent() {
        PojoCloudEventData<OrderConfirmedEvent> cloudEventData = mapData(
                this.event,
                PojoCloudEventDataMapper.from(this.mapper, OrderConfirmedEvent.class));
        
        return cloudEventData.getValue();
    }

    private String getParentTraceId() {
        return event.getExtension("ddtraceid").toString();
    }

    private String getParentSpanId() {
        return event.getExtension("ddspanid").toString();
    }
}
