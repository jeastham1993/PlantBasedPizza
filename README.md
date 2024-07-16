# Plant Based Pizza

> **IMPORTANT! Deploying resources in this repo may incur costs in your own AWS accounts.**

A reference serverless application implementing an online pizza restaurant using an event-driven architecture. If you want to build along with me, I'll be live streaming weekly on [Twitch](https://www.twitch.tv/plant_powered_james) and [YouTube](https://youtube.com/@serverlessjames) as I add more functionality to the application. Links to all live stream recordings and schedule can be found [here](./docs/livestream-recordings.md).

![High Level Architecture Diagram](img/high-level-arch.png)

![Plant Based Pizza homepage screenshot](img/webpage.png)

The application demonstrates various best practices and ways of building serverless applications across a range of different runtimes. The backend is made up of 7 seperate services:

- Orders (.NET) [![.github/workflows/build-order-services.yaml](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-order-services.yaml/badge.svg?branch=main)](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-order-services.yaml)
- [Kitchen (NodeJS)](./src/PlantBasedPizza.Kitchen.Node/README.md)[![.github/workflows/build-kitchen-services-node.yaml](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-kitchen-services-node.yaml/badge.svg?branch=main)](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-kitchen-services-node.yaml)
- Recipes (Java)[![.github/workflows/build-recipe-services-java.yaml](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-recipe-services-java.yaml/badge.svg?branch=main)](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-recipe-services-java.yaml)
- Delivery (.NET)[![.github/workflows/build-delivery-services.yaml](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-delivery-services.yaml/badge.svg?branch=main)](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-delivery-services.yaml)
- Payment (.NET) [![.github/workflows/build-payment-services.yaml](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-payment-services.yaml/badge.svg?branch=main)](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-payment-services.yaml)
- Loyalty Points (.NET)[![.github/workflows/build-loyalty-services.yaml](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-loyalty-services.yaml/badge.svg?branch=main)](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-loyalty-services.yaml)
- Account (.NET)[![.github/workflows/build-account-services.yaml](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-account-services.yaml/badge.svg?branch=main)](https://github.com/jeastham1993/PlantBasedPizza/actions/workflows/build-account-services.yaml)

Overtime, each service will run using a different runtime. Architecturally, most of the services follow a similar pattern of running a web workload on ECS Fargate to handle traffic from the UI. The majority of the processing runs asynchronous on AWS Lambda.

> **It is not recommended to purposefully build an application using every possible runtime, this is for example purposes only**

## Deploy

All deployments run through automated CICD pipelines, with integration tests in some of the services. Unit & integration testing, as well as CICD best practices for serverless applications will be added in upcoming streams.

Deployment instructions into your own account to follow

## Understand

![Service Map](img/service-map.png)

The application is fully instrumented using Datadog native observability tooling.

*IMPORTANT: A fully OpenTelemetry enabled implementation is in the works*