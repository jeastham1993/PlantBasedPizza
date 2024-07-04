package com.recipe.functions;

import datadog.trace.api.Trace;
import com.amazonaws.services.lambda.runtime.events.SQSEvent;
import com.amazonaws.services.lambda.runtime.events.SQSBatchResponse;
import com.recipe.core.IRecipeRepository;
import org.springframework.beans.factory.annotation.Autowired;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Function;

public class HandleOrderConfirmedEventFunction implements Function<SQSEvent, SQSBatchResponse> {
    @Autowired
    IRecipeRepository recipeRepository;
    
    @Override
    public SQSBatchResponse apply(SQSEvent evt) {
        List<SQSBatchResponse.BatchItemFailure> failures = new ArrayList<>();

        List<SQSEvent.SQSMessage> records = evt.getRecords();
        
        for (int i = 0, recordsSize = records.size(); i < recordsSize; i++) {
            SQSEvent.SQSMessage message = records.get(i);
            
            var processSuccess = processMessage(message);
            
            if (!processSuccess) {
                failures.add(SQSBatchResponse.BatchItemFailure.builder().withItemIdentifier(message.getMessageId()).build());
            }
        }

        return SQSBatchResponse.builder()
                .withBatchItemFailures(failures)
                .build();
    }

    @Trace(operationName = "ProcessingMessage", resourceName = "HandleOrderConfirmedEventFunction.processMessage")
    public boolean processMessage(SQSEvent.SQSMessage message) {
        return true;
    }
}