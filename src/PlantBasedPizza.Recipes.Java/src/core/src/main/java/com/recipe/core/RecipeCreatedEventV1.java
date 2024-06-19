package com.recipe.core;

public class RecipeCreatedEventV1 {
    private final long recipeId;

    public RecipeCreatedEventV1(long recipeId){
        this.recipeId = recipeId;
    }

    public long getRecipeId() {
        return recipeId;
    }
}
