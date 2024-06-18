package com.recipe.api.core;

import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.CrudRepository;
import org.springframework.data.repository.query.Param;

import java.util.List;

public interface IRecipeRepository extends CrudRepository<Recipe, Long> {
//    @Query("SELECT r FROM recipes r where r.name = :name")
//    List<Recipe> findByName(@Param("name") String name);
}
