package com.recipe.core;

import java.util.List;
import java.util.Optional;

public interface RecipeCache {
    void SetRecipes(List<RecipeDTO> recipes);
    Optional<List<RecipeDTO>> GetRecipes();

    void SetRecipe(RecipeDTO recipe);
    Optional<RecipeDTO> GetRecipe(String key);
}
