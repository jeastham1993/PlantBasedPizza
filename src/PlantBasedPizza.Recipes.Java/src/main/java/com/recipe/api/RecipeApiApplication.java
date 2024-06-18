package com.recipe.api;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.data.jpa.repository.config.EnableJpaAuditing;

@SpringBootApplication()
@EnableJpaAuditing(auditorAwareRef = "auditorAware")
public class RecipeApiApplication {
    private static final Logger LOG = LogManager.getLogger();

    public static void main(String[] args) {
        LOG.info("Application startup");

        SpringApplication.run(RecipeApiApplication.class, args);
    }
}
