package com.recipe.functions;

import java.util.function.Function;

import com.recipe.core.IRecipeRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

@SpringBootApplication(scanBasePackages = "com.recipe")
public class FunctionConfiguration {
    @Autowired
    IRecipeRepository recipeRepository;

    public static void main(String[] args) {
    }

    @Bean
    public Function<String, String> uppercase() {
        return value -> value.toUpperCase();
    }
}