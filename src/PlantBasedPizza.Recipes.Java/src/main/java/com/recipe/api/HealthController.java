package com.recipe.api;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class HealthController {
    private static final Logger LOG = LogManager.getLogger();
    @GetMapping("/recipe/health")
    ResponseEntity<String> listRecipes() {

        return ResponseEntity.ok("");
    }
}
