package com.recipe.api.core;

import com.recipe.api.messaging.EventBridgeEventPublisher;
import org.springframework.stereotype.Service;

import java.util.ArrayList;

import datadog.trace.api.Trace;


@Service
public class RecipeService {
    private final IRecipeRepository recipeRepository;
    private final EventBridgeEventPublisher eventPublisher;

    public RecipeService(IRecipeRepository recipeRepository, EventBridgeEventPublisher eventPublisher){
        this.recipeRepository = recipeRepository;
        this.eventPublisher = eventPublisher;
    }

    @Trace(operationName = "CreateRecipe", resourceName = "RecipeService.GetRecipe")
    public RecipeDTO CreateRecipe(RecipeDTO recipeDTO) {
        var recipe = new Recipe();
        recipe.setName(recipeDTO.getName());
        recipe.setPrice(recipeDTO.getPrice());

        var savedRecipe = this.recipeRepository.save(recipe);

        eventPublisher.publish(new RecipeCreatedEventV1(recipe.getId()));

        return savedRecipe.asDto();
    }

    @Trace(operationName = "GetRecipe", resourceName = "RecipeService.GetRecipe")
    public RecipeDTO GetRecipe(long recipeId) {
        var retrievedRecipe = this.recipeRepository.findById(recipeId);

        return retrievedRecipe.map(Recipe::asDto).orElse(null);

    }

    @Trace(operationName = "GetRecipe", resourceName = "RecipeService.GetRecipe")
    public Iterable<RecipeDTO> ListRecipes() {
        var recipes = this.recipeRepository.findAll();

        var recipeDtoList = new ArrayList<RecipeDTO>();

        var recipeIterator = recipes.iterator();

        while (recipeIterator.hasNext())
        {
            recipeDtoList.add(recipeIterator.next().asDto());
        }

        return recipeDtoList;
    }
}