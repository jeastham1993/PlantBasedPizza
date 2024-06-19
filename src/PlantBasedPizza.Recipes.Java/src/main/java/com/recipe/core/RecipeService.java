package com.recipe.core;

import com.recipe.api.messaging.EventBridgeEventPublisher;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.stereotype.Service;

import java.util.*;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import java.util.stream.StreamSupport;

import datadog.trace.api.Trace;


@Service
public class RecipeService {
    private final IRecipeRepository recipeRepository;
    private final IIngredientRepository ingredientRepository;
    private final EventBridgeEventPublisher eventPublisher;

    public RecipeService(IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository, EventBridgeEventPublisher eventPublisher){
        this.recipeRepository = recipeRepository;
        this.ingredientRepository = ingredientRepository;
        this.eventPublisher = eventPublisher;
    }

    @Trace(operationName = "CreateRecipe", resourceName = "RecipeService.GetRecipe")
    public Optional<RecipeDTO> CreateRecipe(RecipeDTO recipeDTO) {
        var existingRecipes = this.recipeRepository.findByName(recipeDTO.getName());

        //TODO: Update to re-add findByName
        if (!existingRecipes.isEmpty()) {
            return Optional.empty();
        }

        var recipe = new RecipeBuilder()
                .withCategory(recipeDTO.getCategory())
                .withName(recipeDTO.getName())
                .withPrice(recipeDTO.getPrice())
                .withIngredients(recipeDTO.getIngredients())
                .build();

        var savedRecipe = this.recipeRepository.save(recipe);

        eventPublisher.publish(new RecipeCreatedEventV1(recipe.getId()));

        return Optional.of(savedRecipe.asDto());
    }

    @Trace(operationName = "GetRecipe", resourceName = "RecipeService.GetRecipe")
    public RecipeDTO GetRecipe(long recipeId) {
        var retrievedRecipe = this.recipeRepository.findById(recipeId);

        return retrievedRecipe.map(Recipe::asDto).orElse(null);

    }

    @Trace(operationName = "UpdateRecipe", resourceName = "RecipeService.UpdateRecipe")
    public Optional<RecipeDTO> UpdateRecipe(long recipeId, RecipeDTO recipe) {
        var retrievedRecipe = this.recipeRepository.findById(recipeId);

        //TODO: Update to re-add findByName
        if (retrievedRecipe.isEmpty()) {
            return Optional.empty();
        }

        Recipe existingRecipe = retrievedRecipe.get();
        existingRecipe.getIngredients().forEach(this.ingredientRepository::delete);

        List<Ingredient> newIngredients = new ArrayList<>();
        recipe.getIngredients().forEach(ingredientDTO -> {
            Ingredient ingredient = new Ingredient();
            ingredient.setName(ingredientDTO.getName());
            ingredient.setQuantity(ingredientDTO.getQuantity());
            newIngredients.add(ingredient);
        });

        existingRecipe.setName(recipe.getName());
        existingRecipe.setPrice(recipe.getPrice());
        existingRecipe.setCategory(recipe.getCategory());
        existingRecipe.setIngredients(newIngredients);

        this.recipeRepository.save(existingRecipe);
        eventPublisher.publish(new RecipeUpdatedEventV1(recipe.getId()));

        return Optional.of(retrievedRecipe.map(Recipe::asDto).orElse(null));

    }

    @Trace(operationName = "DeleteRecipe", resourceName = "RecipeService.DeleteRecipe")
    public String DeleteRecipe(long recipeId) {
        this.recipeRepository.deleteById(recipeId);
        eventPublisher.publish(new RecipeDeletedEventV1(recipeId));

        return "OK";

    }

    @Trace(operationName = "GetRecipe", resourceName = "RecipeService.GetRecipe")
    public Iterable<RecipeDTO> ListRecipes() {
        var recipes = this.recipeRepository.findAll();
        Stream<Recipe> recipeStream = StreamSupport.stream(recipes.spliterator(), false);
        List<Recipe> sortedRecipes = recipeStream.sorted(Comparator.comparingInt(Recipe::getOrderCount).reversed()).toList();

        var recipeDtoList = new ArrayList<RecipeDTO>();

        for (Recipe sortedRecipe : sortedRecipes) {
            recipeDtoList.add(sortedRecipe.asDto());
        }

        return recipeDtoList;
    }
}