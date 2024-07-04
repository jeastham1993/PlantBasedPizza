package com.recipes.infrastructure;

import org.jetbrains.annotations.NotNull;
import software.amazon.awscdk.Duration;
import software.amazon.awscdk.Tags;
import software.amazon.awscdk.services.lambda.*;
import software.amazon.awscdk.services.lambda.Runtime;
import software.constructs.Construct;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class BackgroundServices extends Construct {
    public BackgroundServices(@NotNull Construct scope, @NotNull String id, @NotNull BackgroundServiceProps props) {
        super(scope, id);

        Map<String, String> lambdaEnvironment = new HashMap<>();
        lambdaEnvironment.put("MAIN_CLASS", "com.recipe.functions.FunctionConfiguration");
        lambdaEnvironment.put("AWS_LAMBDA_EXEC_WRAPPER", "/opt/datadog_wrapper");
        lambdaEnvironment.put("DD_SITE", "datadoghq.eu");
        lambdaEnvironment.put("DD_SERVICE", props.getSharedProps().getServiceName());
        lambdaEnvironment.put("DD_ENV", props.getSharedProps().getEnvironment());
        lambdaEnvironment.put("DD_VERSION", props.getSharedProps().getVersion());
        lambdaEnvironment.put("DD_API_KEY_SECRET_ARN", props.getDatadogKeyParameter().getSecretArn());
        lambdaEnvironment.put("spring_cloud_function_routingExpression", "handleOrderConfirmedEvent");

        List<ILayerVersion> layers = new ArrayList<>(2);
        layers.add(LayerVersion.fromLayerVersionArn(this, "DatadogJavaLayer", "arn:aws:lambda:eu-west-1:464622532012:layer:dd-trace-java:15"));
        layers.add(LayerVersion.fromLayerVersionArn(this, "DatadogLambdaExtension", "arn:aws:lambda:eu-west-1:464622532012:layer:Datadog-Extension:59"));

        // Create our basic function
        Function lambdaFn = Function.Builder.create(this,"ScheduledFunction")
                .runtime(Runtime.JAVA_21)
                .memorySize(2048)
                .handler("org.springframework.cloud.function.adapter.aws.FunctionInvoker::handleRequest")
                .environment(lambdaEnvironment)
                .timeout(Duration.seconds(30))
                .code(Code.fromAsset("../src/functions/target/com.recipe.functions-0.0.1-SNAPSHOT-aws.jar"))
                .layers(layers)
                .build();

        Tags.of(lambdaFn).add("env", props.getSharedProps().getEnvironment());
        Tags.of(lambdaFn).add("service", props.getSharedProps().getServiceName());
        Tags.of(lambdaFn).add("version", props.getSharedProps().getVersion());

        props.getDatadogKeyParameter().grantRead(lambdaFn);
    }
}
