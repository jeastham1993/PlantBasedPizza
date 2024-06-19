package com.recipe.core;

import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotEmpty;
import jakarta.validation.constraints.NotNull;

import java.util.ArrayList;
import java.util.List;

public class RecipeDTO {
    private long id;

    @NotBlank(message="Name cannot be blank")
    private String name;

    @NotBlank(message="Category cannot be blank")
    private String category;

    @Min(value = 0, message = "Price must be greater than 0")
    @NotNull
    private Double price;

    @NotEmpty(message = "Ingredients must not be empty")
    private List<IngredientDTO> ingredients = new ArrayList<>();

    public long getId() {
        return id;
    }

    public void setId(long id) {
        this.id = id;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public Double getPrice() {
        return price;
    }

    public void setPrice(Double price) {
        this.price = price;
    }

    public String getCategory() {
        return category;
    }

    public void setCategory(String category) {
        this.category = category;
    }

    public List<IngredientDTO> getIngredients() {
        return ingredients;
    }

    public void setIngredients(List<Ingredient> ingredients) {
        ingredients.forEach(ingredient -> {
            IngredientDTO dto = new IngredientDTO();
            dto.setName(ingredient.getName());
            dto.setQuantity(ingredient.getQuantity());
            this.ingredients.add(dto);
        });
    }
}
