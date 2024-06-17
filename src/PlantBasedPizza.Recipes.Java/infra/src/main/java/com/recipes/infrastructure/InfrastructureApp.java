package com.recipes.infrastructure;

import software.amazon.awscdk.App;
import software.amazon.awscdk.Environment;
import software.amazon.awscdk.StackProps;

public class InfrastructureApp {
    public static void main(final String[] args) {
        App app = new App();

        new RecipeJavaInfraStack(app, "RecipeJavaInfraStack", StackProps.builder()
                .env(Environment.builder()
                        .account(System.getenv("CDK_DEFAULT_ACCOUNT"))
                        .region(System.getenv("CDK_DEFAULT_REGION"))
                        .build())
                .build());

        app.synth();
    }
}

