package com.recipe.api;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class HealthController {
    @GetMapping("/recipes/health")
    ResponseEntity<String> healthCheck() {
        return ResponseEntity.ok("");
    }
}
