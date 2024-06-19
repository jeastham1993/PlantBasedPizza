package com.recipe.core;

import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.context.annotation.Configuration;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

@Configuration
@EntityScan("com.recipe.core")
@EnableJpaRepositories(basePackages = "com.recipe.core")
public class RecipeConfiguration {
}
