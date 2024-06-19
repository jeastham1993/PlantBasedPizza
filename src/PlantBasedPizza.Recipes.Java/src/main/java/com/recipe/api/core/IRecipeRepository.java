package com.recipe.api.core;

import org.springframework.data.repository.CrudRepository;

import java.util.List;

public interface IRecipeRepository extends CrudRepository<Recipe, Long> {
    List<Recipe> findByName(String name);
}
