package com.recipe.api.core;

public class RecipeUpdatedEventV1 {
    private final long recipeId;

    public RecipeUpdatedEventV1(long recipeId){
        this.recipeId = recipeId;
    }

    public long getRecipeId() {
        return recipeId;
    }
}
