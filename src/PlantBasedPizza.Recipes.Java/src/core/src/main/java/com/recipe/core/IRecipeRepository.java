package com.recipe.core;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface IRecipeRepository extends CrudRepository<Recipe, Long> {
    List<Recipe> findByName(String name);
}
