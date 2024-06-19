package com.recipe.functions;

import java.util.function.Function;

import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

@SpringBootApplication(scanBasePackages = "com.recipe")
public class FunctionConfiguration {
    public static void main(String[] args) {
        // empty unless using Custom runtime at which point it should include
        // SpringApplication.run(FunctionConfiguration.class, args);
    }

    @Bean
    public Function<String, String> uppercase() {
        return value -> {
            if (value.equals("exception")) {
                throw new RuntimeException("Intentional exception");
            }
            else {
                return value.toUpperCase();
            }
        };
    }
}