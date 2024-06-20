package com.recipe.api;

import com.recipe.core.Recipe;
import com.recipe.core.RecipeBuilder;
import com.recipe.core.RecipeDTO;
import com.recipe.core.RecipeService;
import io.opentracing.Span;
import io.opentracing.util.GlobalTracer;
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
        try {
            Optional<RecipeDTO> createdRecipe = recipeService.CreateRecipe(recipe);

            return createdRecipe.map(ResponseEntity::ok).orElseGet(() -> ResponseEntity.badRequest().body(null));
        } catch (Exception e) {
            return ResponseEntity.internalServerError().body(null);
        }
    }

    @GetMapping("/recipes/")
    ResponseEntity<Iterable<RecipeDTO>> listRecipes() {
        try {
            LOG.info("Received request to retrieve recipes");

            Iterable<RecipeDTO> recipeList = recipeService.ListRecipes();

            LOG.info("Recipe listing successful");

            return ResponseEntity.ok(recipeList);
        } catch (Exception e) {
            return ResponseEntity.internalServerError().body(null);
        }
    }

    @GetMapping("/recipes/{id}")
    ResponseEntity<RecipeDTO> getRecipe(@PathVariable("id") long id) {
        try {
            RecipeDTO recipe = recipeService.GetRecipe(id);

            return ResponseEntity.ok(recipe);
        } catch (Exception e) {

            return ResponseEntity.internalServerError().body(null);
        }
    }

    @DeleteMapping("/recipes/{id}")
    ResponseEntity<String> deleteRecipe(@PathVariable("id") long id) {
        try {
            String deleteResult = recipeService.DeleteRecipe(id);

            return ResponseEntity.ok(deleteResult);
        } catch (Exception e) {
            return ResponseEntity.internalServerError().body(null);
        }
    }

    @PutMapping("/recipes/{id}")
    ResponseEntity<RecipeDTO> updateRecipe(@PathVariable("id") long id, @Valid @RequestBody RecipeDTO recipe) {
        try {
            Optional<RecipeDTO> updatedRecipe = recipeService.UpdateRecipe(id, recipe);

            return updatedRecipe.map(ResponseEntity::ok).orElseGet(() -> ResponseEntity.badRequest().body(null));
        } catch (Exception e) {
            return ResponseEntity.internalServerError().body(null);
        }
    }

    @PostMapping("/recipes/_seed")
    ResponseEntity<String> seedRecipes() {
        try {
            List<Recipe> recipes = new ArrayList<>();

            Recipe margRecipe = new RecipeBuilder()
                    .withName("Margherita")
                    .withPrice(8.99)
                    .withCategory("Pizza")
                    .withIngredient("Base", 1)
                    .withIngredient("Tomatoes", 1)
                    .withIngredient("Margherita", 10)
                    .build();
            Recipe pepperoniRecipe = new RecipeBuilder()
                    .withName("Pepproni")
                    .withPrice(10.99)
                    .withCategory("Pizza")
                    .withIngredient("Base", 1)
                    .withIngredient("Tomatoes", 1)
                    .withIngredient("Margherita", 10)
                    .withIngredient("Pepperoni", 20)
                    .build();
            Recipe veggieRecipe = new RecipeBuilder()
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

            Recipe chickAint = new RecipeBuilder()
                    .withName("Chick-Aint")
                    .withPrice(12.99)
                    .withCategory("Pizza")
                    .withIngredient("Base", 1)
                    .withIngredient("Tomatoes", 1)
                    .withIngredient("Margherita", 10)
                    .withIngredient("Chick-Aint", 10)
                    .withIngredient("Red Peppers", 6)
                    .build();
            Recipe spicyRecipe = new RecipeBuilder()
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

            Recipe friesRecipe = new RecipeBuilder()
                    .withName("Fries")
                    .withPrice(5.99)
                    .withCategory("Sides")
                    .withIngredient("Fries", 1)
                    .build();

            Recipe halloumiSticksRecipe = new RecipeBuilder()
                    .withName("Halloumi Fries")
                    .withPrice(6.99)
                    .withCategory("Sides")
                    .withIngredient("Halloumi Fries", 1)
                    .build();

            Recipe cokeRecipe = new RecipeBuilder()
                    .withName("Coca-Cola")
                    .withPrice(1.49)
                    .withCategory("Drinks")
                    .withIngredient("Coca-Cola", 1)
                    .build();

            Recipe cokeZeroRecipe = new RecipeBuilder()
                    .withName("Coke Zero")
                    .withPrice(1.49)
                    .withCategory("Drinks")
                    .withIngredient("Coke Zero", 1)
                    .build();

            Recipe waterRecipe = new RecipeBuilder()
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
                } catch (Exception e) {
                    ;
                }
            });

            return ResponseEntity.ok("OK");
        } catch (Exception e) {
            return ResponseEntity.internalServerError().body(null);
        }
    }
}
