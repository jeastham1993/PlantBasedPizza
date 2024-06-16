package com.recipe.api.core;

import jakarta.persistence.*;

@Entity
@Table(name = "recipes")
public class Recipe {
    public Recipe()
    {
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private long id;
    private String name;
    private Double price;

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

    public RecipeDTO asDto()
    {
        RecipeDTO dto = new RecipeDTO();
        dto.setId(this.id);
        dto.setName(this.name);
        dto.setPrice(this.price);

        return dto;
    }
}
