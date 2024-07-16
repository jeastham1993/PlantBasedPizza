package com.recipe.core;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.recipe.messaging.EventBridgeEventPublisher;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.stereotype.Service;

import java.util.*;
import java.util.stream.Stream;
import java.util.stream.StreamSupport;

import datadog.trace.api.Trace;

@Service
public class RecipeService {
    private final IRecipeRepository recipeRepository;
    private final IIngredientRepository ingredientRepository;
    private final EventBridgeEventPublisher eventPublisher;
    private final MomentoRecipeCacheImpl recipeCache;
    private final ObjectMapper objectMapper;
    Logger log = LogManager.getLogger(RecipeService.class);

    public RecipeService(IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository, EventBridgeEventPublisher eventPublisher, MomentoRecipeCacheImpl recipeCache, ObjectMapper objectMapper){
        this.recipeRepository = recipeRepository;
        this.ingredientRepository = ingredientRepository;
        this.eventPublisher = eventPublisher;
        this.recipeCache = recipeCache;
        this.objectMapper = objectMapper;
    }

    @Trace(operationName = "CreateRecipe", resourceName = "RecipeService.GetRecipe")
    public Optional<RecipeDTO> CreateRecipe(RecipeDTO recipeDTO) {
        List<Recipe> existingRecipes = this.recipeRepository.findByName(recipeDTO.getName());

        //TODO: Update to re-add findByName
        if (!existingRecipes.isEmpty()) {
            return Optional.empty();
        }

        Recipe recipe = new RecipeBuilder()
                .withCategory(recipeDTO.getCategory())
                .withName(recipeDTO.getName())
                .withPrice(recipeDTO.getPrice())
                .withIngredients(recipeDTO.getIngredients())
                .build();

        Recipe savedRecipe = this.recipeRepository.save(recipe);

        eventPublisher.publish(new RecipeCreatedEventV1(recipe.getId()));

        return Optional.of(savedRecipe.asDto());
    }

    @Trace(operationName = "GetRecipe", resourceName = "RecipeService.GetRecipe")
    public RecipeDTO GetRecipe(long recipeId) {
        var cacheResult = this.recipeCache.Get(String.valueOf(recipeId));

        try {
            if (cacheResult.isPresent()){
                log.info("Cache hit...");
                RecipeDTO recipe = this.objectMapper.readValue(cacheResult.get(), RecipeDTO.class);

                return recipe;
            }
        }
        catch (JsonProcessingException e) {
            log.error(e);
        }

        Optional<Recipe> retrievedRecipe = this.recipeRepository.findById(recipeId);

        var recipeDto = retrievedRecipe.map(Recipe::asDto).orElse(null);

        try {
            log.info("Updating cache...");
            this.recipeCache.Set(String.valueOf(recipeId), this.objectMapper.writeValueAsString(recipeDto));
        }
        catch (JsonProcessingException ex){
            log.error(ex);
        }

        return recipeDto;

    }

    @Trace(operationName = "UpdateRecipe", resourceName = "RecipeService.UpdateRecipe")
    public Optional<RecipeDTO> UpdateRecipe(long recipeId, RecipeDTO recipe) {
        Optional<Recipe> retrievedRecipe = this.recipeRepository.findById(recipeId);

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

    @Trace(operationName = "ListRecipes", resourceName = "RecipeService.ListRecipes")
    public Iterable<RecipeDTO> ListRecipes() {
        var cacheResult = this.recipeCache.Get("all-recipes");

        try {
            if (cacheResult.isPresent()){
                log.info("Cache hit...");
                List<RecipeDTO> recipeList = this.objectMapper.readValue(cacheResult.get(), new TypeReference<>() {});

                return recipeList;
            }
        }
        catch (JsonProcessingException e) {
            log.error(e);
        }

        log.info("Cache miss...");

        Iterable<Recipe> recipes = this.recipeRepository.findAll();
        Stream<Recipe> recipeStream = StreamSupport.stream(recipes.spliterator(), false);
        List<Recipe> sortedRecipes = recipeStream.sorted(Comparator.comparingInt(Recipe::getOrderCount).reversed()).toList();

        List<RecipeDTO> recipeDtoList = new ArrayList<RecipeDTO>();

        for (Recipe sortedRecipe : sortedRecipes) {
            recipeDtoList.add(sortedRecipe.asDto());
        }

        try {
            log.info("Updating cache...");
            this.recipeCache.Set("all-recipes", this.objectMapper.writeValueAsString(recipeDtoList));
        }
        catch (JsonProcessingException ex){
            log.error(ex);
        }

        return recipeDtoList;
    }
}