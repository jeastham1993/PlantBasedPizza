package com.recipe.api.core;

import jakarta.persistence.*;

@Entity
@Table(name = "ingredients")
public class Ingredient {
    public Ingredient()
    {
    }

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private long id;
    private String name;
    private Number quantity;

    @ManyToOne
    @JoinColumn(name="recipe_id", nullable=false, referencedColumnName = "id")
    private Recipe recipe;

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

    public Number getQuantity() {
        return quantity;
    }

    public void setQuantity(Number price) {
        this.quantity = price;
    }

    public Recipe getRecipe() { return recipe; }
}
