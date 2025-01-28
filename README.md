# Plant Based Pizza on Azure

This branch demonstrates how you can build and deploy an event-driven system on Microsoft Azure using a combination of Azure Container Apps & Dapr. 

If you aren't familiar, Azure Container Apps is a serverless container orchestrator built as an abstraction on top of Kubernetes. It simplifies the deployment of containerized applications on Microsoft Azure.

[Dapr](https://dapr.io/) provides integrated APIs for communication, state, and workflow. Dapr leverages industry best practices for security, resiliency, and observability, so you can focus on your code.

The beauty of this combo, is that Dapr is fully integrated inside Azure Container Apps.

## Running Locally

This repo allows you to run the entire PlantBasedPizza application locally. It is fully instrumented with OpenTelemetry and telemetry data is sent to [Jaeger](http://localhost:4317).

To run the application locally, you first need to the container images for the individual microservices and then you can startup the application and all the required infrastructure using the [docker-compose.yml](./docker-compose.yml) file in the root of the repository.

### Build

There is a [Makefile](./Makefile) in the root of the repository that contains all of the required commands to get the application running. The easiest way to get started is to install Make, and then run either:

```sh
make build # if you're running an x86 machine
make build-arm # if you're running an ARM based machine
```

There are several microservices, so those build commands will take a little while. Go and grab yourself a cup of tea or coffee whilst you wait ☕️

Once all the container images are built, you can run:

```sh
docker-compose up -d
```

This will start up the applications, infrastructure, Dapr and a frontend application locally on your machine.

## Deploy to Azure

*Instructions to follow*