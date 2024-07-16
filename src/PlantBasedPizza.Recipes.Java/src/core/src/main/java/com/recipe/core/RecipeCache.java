package com.recipe.core;

import java.util.Optional;

public interface RecipeCache {
    void Set (String key, String value);
    Optional<String> Get (String key);
}
