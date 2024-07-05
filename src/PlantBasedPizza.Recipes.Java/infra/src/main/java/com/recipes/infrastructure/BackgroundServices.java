package com.recipes.infrastructure;

import org.jetbrains.annotations.NotNull;
import software.amazon.awscdk.Duration;
import software.amazon.awscdk.Tags;
import software.amazon.awscdk.services.lambda.*;
import software.amazon.awscdk.services.lambda.Runtime;
import software.amazon.awscdk.services.lambda.eventsources.SqsEventSource;
import software.constructs.Construct;

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
        lambdaEnvironment.put("AWS_LAMBDA_EXEC_WRAPPER", "/opt/datadog_wrapper");
        lambdaEnvironment.put("DD_SITE", "datadoghq.eu");
        lambdaEnvironment.put("DD_SERVICE", props.getSharedProps().getServiceName());
        lambdaEnvironment.put("DD_ENV", props.getSharedProps().getEnvironment());
        lambdaEnvironment.put("DD_VERSION", props.getSharedProps().getVersion());
        lambdaEnvironment.put("DD_API_KEY_SECRET_ARN", props.getDatadogKeyParameter().getSecretArn());
        lambdaEnvironment.put("DB_PARAMETER_NAME", props.getDbConnectionParameter().getParameterName());
        lambdaEnvironment.put("spring_cloud_function_routingExpression", "handleOrderConfirmedEvent");

        List<ILayerVersion> layers = new ArrayList<>(2);
        layers.add(LayerVersion.fromLayerVersionArn(this, "DatadogJavaLayer", "arn:aws:lambda:eu-west-1:464622532012:layer:dd-trace-java:15"));
        layers.add(LayerVersion.fromLayerVersionArn(this, "DatadogLambdaExtension", "arn:aws:lambda:eu-west-1:464622532012:layer:Datadog-Extension:59"));

        // Create our basic function
        Function orderConfirmedHandlerFunction = Function.Builder.create(this,"OrderConfirmedHandler")
                .runtime(Runtime.JAVA_21)
                .memorySize(2048)
                .handler("org.springframework.cloud.function.adapter.aws.FunctionInvoker::handleRequest")
                .environment(lambdaEnvironment)
                .timeout(Duration.seconds(30))
                .code(Code.fromAsset("../src/functions/target/com.recipe.functions-0.0.1-SNAPSHOT-aws.jar"))
                .layers(layers)
                .build();

        Tags.of(orderConfirmedHandlerFunction).add("env", props.getSharedProps().getEnvironment());
        Tags.of(orderConfirmedHandlerFunction).add("service", props.getSharedProps().getServiceName());
        Tags.of(orderConfirmedHandlerFunction).add("version", props.getSharedProps().getVersion());

        props.getDatadogKeyParameter().grantRead(orderConfirmedHandlerFunction);
        props.getDbConnectionParameter().grantRead(orderConfirmedHandlerFunction);

        orderConfirmedHandlerFunction.addEventSource(new SqsEventSource(orderConfirmedQueue.getQueue()));
    }
}
