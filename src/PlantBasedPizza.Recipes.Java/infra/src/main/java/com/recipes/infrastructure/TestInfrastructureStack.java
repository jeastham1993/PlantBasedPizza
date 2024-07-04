package com.recipes.infrastructure;

import software.amazon.awscdk.services.secretsmanager.ISecret;
import software.amazon.awscdk.services.secretsmanager.Secret;
import software.constructs.Construct;
import software.amazon.awscdk.Stack;
import software.amazon.awscdk.StackProps;

public class TestInfrastructureStack extends Stack {
    public TestInfrastructureStack(final Construct scope, final String id) {
        this(scope, id, null);
    }

    public TestInfrastructureStack(final Construct scope, final String id, final StackProps props) {
        SharedProps sharedProps = new SharedProps("integration-test", "RecipeService", "integration-test");
        
        ISecret ddApiKeySecret = Secret.fromSecretNameV2(this, "TestDDApiKeySecret", "DdApiKeySecret-EAtKjZYFq40D");
        
        BackgroundServices services = new BackgroundServices(this, "TestBackgroundServices", new BackgroundServiceProps(sharedProps, ddApiKeySecret));
    }
}
