package com.recipe.functions.services;

import com.recipe.core.IRecipeRepository;
import com.recipe.core.Recipe;
import com.recipe.functions.FunctionConfiguration;
import com.recipe.functions.events.OrderConfirmedEvent;
import com.recipe.functions.events.OrderConfirmedEventItem;
import io.opentracing.Span;
import io.opentracing.util.GlobalTracer;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@Service
public class EventHandlerService implements IEventHandlerService {

    private final IRecipeRepository recipeRepository;
    
    @Autowired
    public EventHandlerService(IRecipeRepository recipeRepository){

        this.recipeRepository = recipeRepository;
    }

    Logger log = LogManager.getLogger(EventHandlerService.class);
    
    public boolean handle(OrderConfirmedEvent event) {
        final Span span = GlobalTracer.get().activeSpan();
        
        String orderIdentifier = event.getOrderIdentifier();

        if (span != null){
            span.setTag("order.orderIdentifier", orderIdentifier);
            span.setTag("order.itemCount", event.getItems().stream().count());
        }

        List<OrderConfirmedEventItem> items = event.getItems();

        for (int i = 0, recordsSize = items.size(); i < recordsSize; i++) {
            OrderConfirmedEventItem item = items.get(i);

            Span recipeProcessingSpan = GlobalTracer.get().buildSpan(String.format("Processing recipe: %s", item.getRecipeIdentifier()))
                    .asChildOf(span)
                    .start();
            recipeProcessingSpan.setTag("order.recipeIdetifier", item.getRecipeIdentifier());

            Optional<Recipe> recipe = this.recipeRepository.findById(Long.parseLong(item.getRecipeIdentifier()));

            if (recipe.isEmpty()){
                recipeProcessingSpan.setTag("order.recipeNotFound", true);
                
                log.info("Recipe not found");
                continue;
            }

            Recipe unwrappedRecipe = recipe.get();
            unwrappedRecipe.incrementOrderCount();

            this.recipeRepository.save(unwrappedRecipe);

            log.info(String.format("New count is %s", unwrappedRecipe.getOrderCount()));
            
            recipeProcessingSpan.finish();
        }
        
        return true;
    }
}
