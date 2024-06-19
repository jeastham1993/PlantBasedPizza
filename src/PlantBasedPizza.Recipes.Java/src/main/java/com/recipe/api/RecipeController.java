package com.recipe.api;

import com.recipe.api.core.*;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import jakarta.validation.Valid;

import java.util.*;

@RestController
public class RecipeController {
    private final RecipeService recipeService;

    private static final Logger LOG = LogManager.getLogger();

    public RecipeController(RecipeService recipeService) {
        this.recipeService = recipeService;
    }

    @PostMapping("/recipes/")
    ResponseEntity<RecipeDTO> addRecipe(@Valid @RequestBody RecipeDTO recipe) {
        var createdRecipe = recipeService.CreateRecipe(recipe);

        return createdRecipe.map(ResponseEntity::ok).orElseGet(() -> ResponseEntity.badRequest().body(null));
    }

    @GetMapping("/recipes/")
    ResponseEntity<Iterable<RecipeDTO>> listRecipes() {
        LOG.info("Received request to retrieve recipes");

        var recipeList = recipeService.ListRecipes();

        LOG.info("Recipe listing successful");

        return ResponseEntity.ok(recipeList);
    }

    @GetMapping("/recipes/{id}")
    ResponseEntity<RecipeDTO> getRecipe(@PathVariable long id) {
        var recipe = recipeService.GetRecipe(id);

        return ResponseEntity.ok(recipe);
    }

    @DeleteMapping("/recipes/{id}")
    ResponseEntity<String> deleteRecipe(@PathVariable long id) {
        var deleteResult = recipeService.DeleteRecipe(id);

        return ResponseEntity.ok(deleteResult);
    }

    @PutMapping("/recipes/{id}")
    ResponseEntity<RecipeDTO> updateRecipe(@PathVariable long id, @Valid @RequestBody RecipeDTO recipe) {
        var updatedRecipe = recipeService.UpdateRecipe(id, recipe);

        return updatedRecipe.map(ResponseEntity::ok).orElseGet(() -> ResponseEntity.badRequest().body(null));
    }

    @PostMapping("/recipes/_seed")
    ResponseEntity<String> seedRecipes() {

        List<Recipe> recipes = new ArrayList<>();

        var margRecipe = new RecipeBuilder()
                .withName("Margherita")
                .withPrice(8.99)
                .withCategory("Pizza")
                .withIngredient("Base", 1)
                .withIngredient("Tomatoes", 1)
                .withIngredient("Margherita", 10)
                .build();
        var pepperoniRecipe = new RecipeBuilder()
                .withName("Pepproni")
                .withPrice(10.99)
                .withCategory("Pizza")
                .withIngredient("Base", 1)
                .withIngredient("Tomatoes", 1)
                .withIngredient("Margherita", 10)
                .withIngredient("Pepperoni", 20)
                .build();
        var veggieRecipe = new RecipeBuilder()
                .withName("Veggie Deluxe")
                .withPrice(10.99)
                .withCategory("Pizza")
                .withIngredient("Base", 1)
                .withIngredient("Tomatoes", 1)
                .withIngredient("Margherita", 10)
                .withIngredient("Red Peppers", 6)
                .withIngredient("Green Peppers", 6)
                .withIngredient("Mushrooms", 6)
                .withIngredient("Olives", 12)
                .build();

        var chickAint = new RecipeBuilder()
                .withName("Chick-Aint")
                .withPrice(12.99)
                .withCategory("Pizza")
                .withIngredient("Base", 1)
                .withIngredient("Tomatoes", 1)
                .withIngredient("Margherita", 10)
                .withIngredient("Chick-Aint", 10)
                .withIngredient("Red Peppers", 6)
                .build();
        var spicyRecipe = new RecipeBuilder()
                .withName("Hot N Spicy")
                .withPrice(10.99)
                .withCategory("Pizza")
                .withIngredient("Base", 1)
                .withIngredient("Tomatoes", 1)
                .withIngredient("Margherita", 10)
                .withIngredient("Mushrooms", 6)
                .withIngredient("Jalapenos", 12)
                .withIngredient("NDuja", 6)
                .build();

        var friesRecipe = new RecipeBuilder()
                .withName("Fries")
                .withPrice(5.99)
                .withCategory("Sides")
                .withIngredient("Fries", 1)
                .build();

        var halloumiSticksRecipe = new RecipeBuilder()
                .withName("Halloumi Fries")
                .withPrice(6.99)
                .withCategory("Sides")
                .withIngredient("Halloumi Fries", 1)
                .build();

        var cokeRecipe = new RecipeBuilder()
                .withName("Coca-Cola")
                .withPrice(1.49)
                .withCategory("Drinks")
                .withIngredient("Coca-Cola", 1)
                .build();

        var cokeZeroRecipe = new RecipeBuilder()
                .withName("Coke Zero")
                .withPrice(1.49)
                .withCategory("Drinks")
                .withIngredient("Coke Zero", 1)
                .build();

        var waterRecipe = new RecipeBuilder()
                .withName("Water")
                .withPrice(1.19)
                .withCategory("Drinks")
                .withIngredient("Water", 1)
                .build();

        recipes.add(margRecipe);
        recipes.add(pepperoniRecipe);
        recipes.add(veggieRecipe);
        recipes.add(chickAint);
        recipes.add(spicyRecipe);
        recipes.add(friesRecipe);
        recipes.add(halloumiSticksRecipe);
        recipes.add(cokeRecipe);
        recipes.add(cokeZeroRecipe);
        recipes.add(waterRecipe);

        recipes.forEach(recipe -> {
            try {
                this.recipeService.CreateRecipe(recipe.asDto());
            }
            catch (Exception e) {;
            }
        });

        return ResponseEntity.ok("OK");
    }
}
