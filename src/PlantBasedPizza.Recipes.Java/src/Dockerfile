FROM amazoncorretto:21-alpine
VOLUME /tmp
RUN apk --no-cache add curl
#Uncomment to add the datadog tracing library
RUN curl -Lo dd-java-agent.jar https://dtdg.co/latest-java-tracer 
COPY api/target/*.jar app.jar
ARG DD_APM_IGNORE_RESOURCES=(GET) /recipes/health
ENTRYPOINT ["java","-javaagent:../dd-java-agent.jar", "-Ddd.iast.enabled=true", "-Ddd.trace.sample.rate=1", "-Ddd.profiling.enabled=true", "-XX:FlightRecorderOptions=stackdepth=256", "-Ddd.profiling.ddprof.cpu.enabled=true", "-jar","/app.jar"]