package com.recipes.infrastructure;

import org.jetbrains.annotations.NotNull;
import software.amazon.awscdk.Duration;
import software.amazon.awscdk.Tags;
import software.amazon.awscdk.services.ecr.IRepository;
import software.amazon.awscdk.services.ecr.Repository;
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

        IRepository repository = Repository.fromRepositoryName(this, props.getSharedProps().getServiceName() + "Repo", "recipe-functions-java");
        
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
        
        // Create our basic function
        Function orderConfirmedHandlerFunction = Function.Builder.create(this,"OrderConfirmedEventHandler")
                .runtime(Runtime.FROM_IMAGE)
                .memorySize(2048)
                .handler(Handler.FROM_IMAGE)
                .environment(lambdaEnvironment)
                .timeout(Duration.seconds(30))
                .code(Code.fromEcrImage(repository, EcrImageCodeProps.builder()
                                .tagOrDigest(props.getTag() != null ? props.getTag() : "latest")
                        .build()))
                .build();

        Tags.of(orderConfirmedHandlerFunction).add("env", props.getSharedProps().getEnvironment());
        Tags.of(orderConfirmedHandlerFunction).add("service", props.getSharedProps().getServiceName());
        Tags.of(orderConfirmedHandlerFunction).add("version", props.getSharedProps().getVersion());

        props.getDatadogKeyParameter().grantRead(orderConfirmedHandlerFunction);
        props.getDbConnectionParameter().grantRead(orderConfirmedHandlerFunction);

        orderConfirmedHandlerFunction.addEventSource(new SqsEventSource(orderConfirmedQueue.getQueue()));
    }
}
