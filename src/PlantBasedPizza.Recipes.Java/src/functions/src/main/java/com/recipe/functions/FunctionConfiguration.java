package com.recipe.functions;

import com.amazonaws.services.lambda.runtime.events.CloudWatchLogsEvent;
import com.amazonaws.services.lambda.runtime.events.SQSEvent;
import com.amazonaws.services.lambda.runtime.events.SQSBatchResponse;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.recipe.core.IRecipeRepository;
import com.recipe.functions.events.OrderConfirmedEvent;
import datadog.trace.api.Trace;
import io.cloudevents.CloudEvent;
import io.cloudevents.core.data.PojoCloudEventData;
import io.cloudevents.core.format.EventFormat;
import io.cloudevents.core.provider.EventFormatProvider;
import io.cloudevents.jackson.JsonFormat;
import io.cloudevents.jackson.PojoCloudEventDataMapper;
import io.opentracing.Span;
import io.opentracing.util.GlobalTracer;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import static io.cloudevents.core.CloudEventUtils.mapData;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.Function;

@SpringBootApplication(scanBasePackages = "com.recipe")
public class FunctionConfiguration{
    @Autowired
    IRecipeRepository recipeRepository;
    
    ObjectMapper objectMapper = new ObjectMapper();

    Logger log = LogManager.getLogger(FunctionConfiguration.class);

    public static void main(String[] args) {
        SpringApplication.run(FunctionConfiguration.class, args);
    } 
    
    @Bean
    public Function<SQSEvent, SQSBatchResponse> handleOrderConfirmedEvent() {
        return value -> {
            List<SQSBatchResponse.BatchItemFailure> failures = new ArrayList<>();

            List<SQSEvent.SQSMessage> records = value.getRecords();

            for (int i = 0, recordsSize = records.size(); i < recordsSize; i++) {
                SQSEvent.SQSMessage message = records.get(i);

                var processSuccess = processOrderConfirmedMessage(message);

                if (!processSuccess) {
                    failures.add(SQSBatchResponse.BatchItemFailure.builder().withItemIdentifier(message.getMessageId()).build());
                }
            }
            
            return SQSBatchResponse.builder()
                    .withBatchItemFailures(failures)
                    .build();
        };
    }

    @Trace(operationName = "ProcessingOrderConfirmedMessage", resourceName = "FunctionConfiguration.processOrderConfirmedMessage")
    public boolean processOrderConfirmedMessage(SQSEvent.SQSMessage message) {
        final Span span = GlobalTracer.get().activeSpan();
        
        try {
            if (span != null){
             span.setTag("sqs.messageId", message.getMessageId());   
            }
            
            log.info(String.format("Processing message %s", message.getMessageId()));
            
            Map<String, Object> wrapper = this.objectMapper.readValue(message.getBody(), HashMap.class);

            EventFormat format = EventFormatProvider
                    .getInstance()
                    .resolveFormat(JsonFormat.CONTENT_TYPE);
            
            Object value = wrapper.get("detail");
            
            String valueString = this.objectMapper.writeValueAsString(value);
            
            CloudEvent event = format.deserialize(valueString.getBytes(StandardCharsets.UTF_8));

            PojoCloudEventData<OrderConfirmedEvent> cloudEventData = mapData(
                    event,
                    PojoCloudEventDataMapper.from(this.objectMapper, OrderConfirmedEvent.class));

            String orderIdentifier = cloudEventData.getValue().getOrderIdentifier();

            if (span != null){
                span.setTag("order.orderIdentifier", orderIdentifier);
            }
        }
        catch (Exception exception){
            log.error(exception);
            return false;
        }
        
        return true;
    }
}