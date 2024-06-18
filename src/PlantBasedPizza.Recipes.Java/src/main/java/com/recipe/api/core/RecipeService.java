package com.recipe.api.core;

import com.recipe.api.messaging.EventBridgeEventPublisher;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.Optional;

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
    public Optional<RecipeDTO> CreateRecipe(RecipeDTO recipeDTO) {
        //var existingRecipes = this.recipeRepository.findByName(recipeDTO.getName());

        //TODO: Update to re-add findByName
        if (true == false) {
            return Optional.empty();
        }

        var recipe = new Recipe();
        recipe.setName(recipeDTO.getName());
        recipe.setPrice(recipeDTO.getPrice());
        recipe.setCategory(recipeDTO.getCategory());

        var savedRecipe = this.recipeRepository.save(recipe);

        eventPublisher.publish(new RecipeCreatedEventV1(recipe.getId()));

        return Optional.of(savedRecipe.asDto());
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