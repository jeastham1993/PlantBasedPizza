package com.recipe.api.core;

import org.springframework.data.repository.CrudRepository;

public interface IIngredientRepository extends CrudRepository<Ingredient, Long> {
}
