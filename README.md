# Plant Based Pizza on Azure

This branch demonstrates how you can build and deploy an event-driven system on Microsoft Azure using a combination of Azure Container Apps & Dapr. 

If you aren't familiar, Azure Container Apps is a serverless container orchestrator built as an abstraction on top of Kubernetes. It simplifies the deployment of containerized applications on Microsoft Azure.

[Dapr](https://dapr.io/) provides integrated APIs for communication, state, and workflow. Dapr leverages industry best practices for security, resiliency, and observability, so you can focus on your code.

The beauty of this combo, is that Dapr is fully integrated inside Azure Container Apps.

## Prerequisites

- [.NET9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Docker client
- Make
    - [For Windows](https://gnuwin32.sourceforge.net/packages/make.htm)
    - [For Mac](https://formulae.brew.sh/formula/make)
    - [Linux](https://askubuntu.com/questions/161104/how-do-i-install-make)


## Running Locally

There are several steps to running the application locally:

1. Build the container images for all services: `make build` or `make-build-arm` depending on your system CPU architecture
2. Start the backend service containers and required infrastructure: `docker-compose up -d`, wait for all containers to start and then `docker compose -f docker-compose-services.yml up -d`
3. Access the front-end on [http://localhost:3000](http://localhost:3000)
4. Once up and running you can go and register a new user to start interacting with the system
    - If you are trying to login to the [admin interface](http://localhost:3000/admin/login) a default user is created with credentials `admin@plantbasedpizza.com`:`AdminAccount!23`

## Starting an individual service

All the individual microservices can run independently, and all follow the same structure inside their respective folder under [src](./src/):

1. Start up required infrastructure: `docker-compose up -d`
2. Start up the API component and Dapr sidecar, you'll need two separate terminal windows:
    - `make local-api`
    - `make dapr-api-sidecar`
3. If the specific microservice has a worker component for handling events start them as well, you'll need two more terminal windows:
    - `make local-worker`
    - `make dapr-worker-sidecar`
4. You can switch out either `make local-api` or `make local-worker` with starting the application inside your IDE in debug mode

This will run the individual microservice locally.

## Deploy to Azure

*Instructions to follow*