FROM amazoncorretto:21-alpine
VOLUME /tmp
RUN apk --no-cache add curl
#Uncomment to add the datadog tracing library
RUN curl -Lo dd-java-agent.jar https://dtdg.co/latest-java-tracer 
COPY target/*.jar app.jar
ENTRYPOINT ["java","-javaagent:../dd-java-agent.jar", "-Ddd.trace.sample.rate=1","-jar","/app.jar"]