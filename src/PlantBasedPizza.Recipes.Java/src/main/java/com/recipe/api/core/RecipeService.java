package com.recipe.api.core;

import org.springframework.stereotype.Service;

import java.util.ArrayList;

import datadog.trace.api.Trace;


@Service
public class RecipeService {
    private final IRecipeRepository recipeRepository;

    public RecipeService(IRecipeRepository recipeRepository){
        this.recipeRepository = recipeRepository;
    }

    @Trace(operationName = "CreateRecipe", resourceName = "RecipeService.GetRecipe")
    public RecipeDTO CreateRecipe(RecipeDTO recipeDTO) {
        var recipe = new Recipe();
        recipe.setName(recipeDTO.getName());
        recipe.setPrice(recipeDTO.getPrice());

        var savedRecipe = this.recipeRepository.save(recipe);

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