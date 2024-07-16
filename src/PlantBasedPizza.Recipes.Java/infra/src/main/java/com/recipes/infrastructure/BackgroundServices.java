package com.recipes.infrastructure;

import org.jetbrains.annotations.NotNull;
import software.amazon.awscdk.Duration;
import software.amazon.awscdk.Tags;
import software.amazon.awscdk.services.lambda.*;
import software.amazon.awscdk.services.lambda.Runtime;
import software.amazon.awscdk.services.lambda.eventsources.SqsEventSource;
import software.amazon.awscdk.services.s3.Bucket;
import software.amazon.awscdk.services.s3.IBucket;
import software.amazon.awscdk.services.s3.assets.Asset;
import software.amazon.awscdk.services.s3.assets.AssetOptions;
import software.constructs.Construct;

import java.io.File;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class BackgroundServices extends Construct {
    public BackgroundServices(@NotNull Construct scope, @NotNull String id, @NotNull BackgroundServiceProps props) {
        super(scope, id);
        EventQueueProps orderConfirmedQueueProps = new EventQueueProps(props.getSharedProps(), props.getBus(), "https://orders.plantbasedpizza/", "order.orderConfirmed.v1", "OrderConfirmed");
        EventQueue orderConfirmedQueue = new EventQueue(this, "OrderConfirmedQueue", orderConfirmedQueueProps);

        Map<String, String> lambdaEnvironment = new HashMap<>();
        lambdaEnvironment.put("MAIN_CLASS", "com.recipe.functions.FunctionConfiguration");
        //lambdaEnvironment.put("AWS_LAMBDA_EXEC_WRAPPER", "/opt/datadog_wrapper");
        lambdaEnvironment.put("DD_SITE", "datadoghq.eu");
        lambdaEnvironment.put("DD_SERVICE", props.getSharedProps().getServiceName());
        lambdaEnvironment.put("DD_ENV", props.getSharedProps().getEnvironment());
        lambdaEnvironment.put("DD_VERSION", props.getSharedProps().getVersion());
        lambdaEnvironment.put("DD_API_KEY_SECRET_ARN", props.getDatadogKeyParameter().getSecretArn());
        lambdaEnvironment.put("DB_PARAMETER_NAME", props.getDbConnectionParameter().getParameterName());
        lambdaEnvironment.put("DD_IAST_ENABLED", "true");

        // Uploaded to Amazon S3 as-is
        Asset fileAsset = Asset.Builder.create(this, "LambdaFunctionJarS3Asset")
                .path("../src/functions/target/com.recipe.functions-0.0.1-SNAPSHOT-aws.jar").build();

        List<ILayerVersion> layers = new ArrayList<>(2);
//        layers.add(LayerVersion.fromLayerVersionArn(this, "DatadogJavaLayer", "arn:aws:lambda:eu-west-1:464622532012:layer:dd-trace-java:15"));
//        layers.add(LayerVersion.fromLayerVersionArn(this, "DatadogLambdaExtension", "arn:aws:lambda:eu-west-1:464622532012:layer:Datadog-Extension:59"));

        IBucket bucket = Bucket.fromBucketName(this, "CDKBucket", fileAsset.getS3BucketName());

        Map<String, String> orderConfirmedEnvironmentVariables = new HashMap<>();
        orderConfirmedEnvironmentVariables.put("spring_cloud_function_definition", "handleOrderConfirmedEvent");
        orderConfirmedEnvironmentVariables.putAll(lambdaEnvironment);
        
        // Create our basic function
        Function orderConfirmedHandlerFunction = Function.Builder.create(this,"OrderConfirmedHandler")
                .runtime(Runtime.JAVA_21)
                .memorySize(2048)
                .handler("org.springframework.cloud.function.adapter.aws.FunctionInvoker::handleRequest")
                .environment(orderConfirmedEnvironmentVariables)
                .timeout(Duration.seconds(30))
                .code(Code.fromBucket(bucket, fileAsset.getS3ObjectKey()))
                .layers(layers)
                .build();

        Tags.of(orderConfirmedHandlerFunction).add("env", props.getSharedProps().getEnvironment());
        Tags.of(orderConfirmedHandlerFunction).add("service", props.getSharedProps().getServiceName());
        Tags.of(orderConfirmedHandlerFunction).add("version", props.getSharedProps().getVersion());

        props.getDatadogKeyParameter().grantRead(orderConfirmedHandlerFunction);
        props.getDbConnectionParameter().grantRead(orderConfirmedHandlerFunction);

        orderConfirmedHandlerFunction.addEventSource(new SqsEventSource(orderConfirmedQueue.getQueue()));

        Map<String, String> recipeCreatedEnvironmentVariables = new HashMap<>();
        recipeCreatedEnvironmentVariables.put("spring_cloud_function_definition", "handleRecipeCreatedEvent");
        recipeCreatedEnvironmentVariables.put("MOMENTO_API_KEY", props.getMomentoApiKey().getStringValue());
        recipeCreatedEnvironmentVariables.put("CACHE_NAME", "plant-based-pizza-recipes");
        recipeCreatedEnvironmentVariables.putAll(lambdaEnvironment);

        // Create our basic function
        Function recipeCreatedHandlerFunction = Function.Builder.create(this,"RecipeCreatedHandler")
                .runtime(Runtime.JAVA_21)
                .memorySize(2048)
                .handler("org.springframework.cloud.function.adapter.aws.FunctionInvoker::handleRequest")
                .environment(recipeCreatedEnvironmentVariables)
                .timeout(Duration.seconds(30))
                .code(Code.fromBucket(bucket, fileAsset.getS3ObjectKey()))
                .layers(layers)
                .build();

        Tags.of(recipeCreatedHandlerFunction).add("env", props.getSharedProps().getEnvironment());
        Tags.of(recipeCreatedHandlerFunction).add("service", props.getSharedProps().getServiceName());
        Tags.of(recipeCreatedHandlerFunction).add("version", props.getSharedProps().getVersion());

        props.getDatadogKeyParameter().grantRead(recipeCreatedHandlerFunction);
        props.getDbConnectionParameter().grantRead(recipeCreatedHandlerFunction);
        props.getMomentoApiKey().grantRead(recipeCreatedHandlerFunction);

        EventQueueProps recipeCreatedQueueProps = new EventQueueProps(props.getSharedProps(), props.getBus(), "https://recipes.plantbasedpizza", "recipe.recipeCreated.v1", "Recipes-RecipeCreatedEvent");
        EventQueue recipeCreatedQueue = new EventQueue(this, "RecipeCreatedQueueProps", recipeCreatedQueueProps);

        recipeCreatedHandlerFunction.addEventSource(new SqsEventSource(recipeCreatedQueue.getQueue()));
    }
}
