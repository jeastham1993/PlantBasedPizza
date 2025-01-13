# Plant Based Pizza

An example of a monolithic implementation of the PlantBasedPizza application, with deployment examples in Azure Container Apps (with Terraform & Pulumi) and ECS Fargate. This example is structured as a modular monolith, with [architecture tests](./src/PlantBasedPizza.Api/tests/PlantBasedPizza.UnitTest/ArchitectureTests.cs) in place to enforce the module to module communication.

The application is also fully instrumented with OpenTelemetry, and the deployment options are configured to send telemetry to [Datadog](https://www.datadoghq.com/).

## Deploy to Azure

[Azure Container Apps (ACA)](https://azure.microsoft.com/en-us/products/container-apps) is used when deploying the application to Azure. ACA provides a simple method to deploy containers onto Azure, using abstractions built on top of Kubernetes. This both provides a simplified developer experience, as well as a good off-ramp if full blown Kubernetes is ever required.

There are examples in this repository using either [Terraform](https://www.terraform.io/) or [Pulumi](https://www.pulumi.com/) as the infrastructure as code (IaC) tool of choice. [Azure Bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview?tabs=bicep) is another option to deploy applications to Azure.

### Terraform

To deploy using Terraform follow the below instructions:

1. Create a [dev.tfvars] file under [./azure](./azure). This is where you all the required variables to deploy PlantBasedPizza
2. Populate `dev.tfvars` with the below configuration options:

```hcl
subscription_id = "" # Your Azure Subscription ID
resource_group_name = "" # The name of an existing resource group to deploy the application into
env = "dev"
app_version = "147fa1f"
db_connection_string = "" # A valid MongoDB connection string. MongoDB Atlas has a generous free tier
dd_site = "" # The Datadog site to send instrumentation details to
dd_api_key = "" # Your Datadog API Key
```

3. Run `terraform apply --var-file dev.tfvars` from inside the [./azure](./azure) folder

### Pulumi

To deploy using Pulumi, follow the below instructions:

1. Create a file named [Pulumi.dev.yaml] under [./azure-pulumi](./azure-pulumi). This is where you all the required variables to deploy PlantBasedPizza
2. Populate `Pulumi.dev.yaml` with the below configuration options:

```yaml
config:
  azure-native:location: westeurope
  PlantBasedPizza.Pulumi:DD_API_KEY: <YOUR_DATADOG_API_KEY>
  PlantBasedPizza.Pulumi:DB_CONNECTION_STRING: <YOUR DATABASE CONNECTION STRING>
  PlantBasedPizza.Pulumi:IMAGE_TAG: 147fa1f
```

3. Run `pulumi up` from inside the [./azure-pulumi](./azure-pulumi) folder


## Deploy to AWS

[Amazon ECS](https://aws.amazon.com/ecs/) and [AWS Fargate](https://aws.amazon.com/fargate/) are used when deploying the application to AWS. ECS + Fargate provides a way to run containers on AWS with little to no operational overhead. Instead, the operations side of running infrastructure is AWS responsibility

There is an example in the repository to deploy the monolithic application to AWS using the [AWS CDK](https://aws.amazon.com/cdk/). The AWS CDK allows you to define cloud resources using a programming language you are familiar with, in this case C#.

### AWS CDK

To deploy using the AWS CDK, follow the below instructions:

1. Set the `DD_API_KEY` and `DATABASE_CONNECTION` environment variables to be your Datadog API key and MongoDB connection string respectively

```sh
export DD_API_KEY=
export DATABASE_CONNECTION=
```

2. Run `cdk deploy` from inside the [./aws/](./aws/) folder