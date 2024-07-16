package com.recipe.core;

public class RecipeCreatedEventV1 {
    private long recipeId;

    public RecipeCreatedEventV1(){
        this.recipeId = -1;
    }

    public RecipeCreatedEventV1(long recipeId){
        this.recipeId = recipeId;
    }

    public long getRecipeId() {
        return recipeId;
    }

    public void setRecipeId(long recipeId) {
        this.recipeId = recipeId;
    }
}
