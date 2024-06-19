package com.recipe.api.core;

import java.util.ArrayList;
import java.util.List;

public class RecipeBuilder {
    private String name;
    private String category;
    private Double price;
    private List<Ingredient> ingredients = new ArrayList<>();

    public RecipeBuilder withName(String name) {
        this.name = name;
        return this;
    }

    public RecipeBuilder withCategory(String category) {
        this.category = category;
        return this;
    }

    public RecipeBuilder withPrice(Double price) {
        this.price = price;
        return this;
    }

    public RecipeBuilder withIngredients(List<IngredientDTO> ingredients) {
        ingredients.forEach(ingredientDTO -> {
            Ingredient ingredient = new Ingredient();
            ingredient.setQuantity(ingredientDTO.getQuantity());
            ingredient.setName(ingredientDTO.getName());
            this.ingredients.add(ingredient);
        });

        return this;
    }

    public RecipeBuilder withIngredient(String name, int quantity) {
            Ingredient ingredient = new Ingredient();
            ingredient.setQuantity(quantity);
            ingredient.setName(name);
            this.ingredients.add(ingredient);

            return this;
    }

    public Recipe build() {
        Recipe recipe = new Recipe();
        recipe.setName(name);
        recipe.setCategory(category);
        recipe.setPrice(price);
        recipe.setIngredients(ingredients != null ? ingredients : new ArrayList<>());
        return recipe;
    }
}
