package com.recipe.api;
import com.google.gson.Gson;
import org.springframework.context.annotation.Configuration;
import software.amazon.awssdk.services.ssm.SsmClient;
import software.amazon.awssdk.services.ssm.model.GetParameterRequest;
import software.amazon.awssdk.services.ssm.model.GetParameterResponse;

@Configuration
public class ApplicationConfiguration {
    private final Gson gson = new Gson();
    private ApplicationProperties properties;

    public ApplicationProperties getApplicationProperties()
    {
        if (properties != null)
        {
            return properties;
        }

        this.properties = getProps();

        return properties;
    }

    private ApplicationProperties getProps() {
        var parameterName = System.getenv("CONFIG_PARAMETER_NAME");
        if (parameterName == null || parameterName == "") {
            return new ApplicationProperties();
        }

        var ssmClient = SsmClient.create();

        String parameter;

        var getParameterRequest = GetParameterRequest.builder()
                .name(parameterName)
                .build();

        GetParameterResponse result = null;

        try {
            result = ssmClient.getParameter(getParameterRequest);
        }
        catch (Exception e) {
            throw e;
        }
        if (result.parameter().value() != null) {
            parameter = result.parameter().value();
            return gson.fromJson(parameter, ApplicationProperties.class);
        }

        return null;
    }
}
