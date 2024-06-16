package com.recipe.api.core;

import org.springframework.data.repository.CrudRepository;

public interface IRecipeRepository extends CrudRepository<Recipe, Long> {
}
