package com.recipe.api;

import com.recipe.api.core.RecipeDTO;
import com.recipe.api.core.RecipeService;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import jakarta.validation.Valid;

@RestController
public class RecipeController {
    private final RecipeService recipeService;
    private final ApplicationProperties applicationProperties;

    private static final Logger LOG = LogManager.getLogger();

    public RecipeController(RecipeService recipeService, ApplicationConfiguration configuration) {
        this.recipeService = recipeService;
        this.applicationProperties = configuration.getApplicationProperties();
    }

    @PostMapping("/recipe/")
    ResponseEntity<RecipeDTO> addRecipe(@Valid @RequestBody RecipeDTO recipe) {
        var createdRecipe = recipeService.CreateRecipe(recipe);

        return ResponseEntity.ok(createdRecipe);
    }

    @GetMapping("/recipe/")
    ResponseEntity<Iterable<RecipeDTO>> listRecipes() {
        LOG.info("Received request to retrieve recipes");

        var recipeList = recipeService.ListRecipes();

        LOG.info("Recipe listing successful");

        return ResponseEntity.ok(recipeList);
    }

    @GetMapping("/recipe/{id}")
    ResponseEntity<RecipeDTO> getRecipe(@PathVariable long id) {
        var recipe = recipeService.GetRecipe(id);

        return ResponseEntity.ok(recipe);
    }
}
