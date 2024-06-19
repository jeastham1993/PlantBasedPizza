package com.recipe.api.core;

public class RecipeDeletedEventV1 {
    private final long recipeId;

    public RecipeDeletedEventV1(long recipeId){
        this.recipeId = recipeId;
    }

    public long getRecipeId() {
        return recipeId;
    }
}
