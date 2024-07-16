package com.recipe.functions.services;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.recipe.core.*;
import com.recipe.functions.FunctionConfiguration;
import com.recipe.functions.events.OrderConfirmedEvent;
import com.recipe.functions.events.OrderConfirmedEventItem;
import io.opentracing.Span;
import io.opentracing.util.GlobalTracer;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.Optional;
import java.util.stream.Stream;
import java.util.stream.StreamSupport;

@Service
public class EventHandlerService implements IEventHandlerService {

    private final IRecipeRepository recipeRepository;
    private final ObjectMapper objectMapper;
    private final RecipeCache recipeCache;
    
    @Autowired
    public EventHandlerService(IRecipeRepository recipeRepository, RecipeCache recipeCache){

        this.recipeRepository = recipeRepository;
        this.objectMapper = new ObjectMapper();
        objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
        this.recipeCache = recipeCache;
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

    @Override
    public boolean handle(RecipeCreatedEventV1 event) {
        // Cache individual recipe
        var recipe = this.recipeRepository.findById(event.getRecipeId());

        this.recipeCache.SetRecipe(recipe.map(Recipe::asDto).get());

        // Update cache of ALL recipes
        var allRecipes = this.recipeRepository.findAll();
        Stream<Recipe> recipeStream = StreamSupport.stream(allRecipes.spliterator(), false);
        List<Recipe> sortedRecipes = recipeStream.sorted(Comparator.comparingInt(Recipe::getOrderCount).reversed()).toList();

        List<RecipeDTO> recipeDtoList = new ArrayList<RecipeDTO>();

        for (Recipe sortedRecipe : sortedRecipes) {
            recipeDtoList.add(sortedRecipe.asDto());
        }

        this.recipeCache.SetRecipes(recipeDtoList);

        return true;
    }
}
